use std::sync::atomic::{AtomicU64, Ordering};
use tauri::{AppHandle, Emitter};

static KEY_COUNT: AtomicU64 = AtomicU64::new(0);

// macOS: C 回调无法捕获变量，用 OnceLock 存 AppHandle
#[cfg(target_os = "macos")]
static APP_HANDLE: std::sync::OnceLock<AppHandle> = std::sync::OnceLock::new();

pub fn total() -> u64 {
    KEY_COUNT.load(Ordering::Relaxed)
}

pub fn start(app: AppHandle) {
    #[cfg(target_os = "macos")]
    start_macos(app);

    #[cfg(not(target_os = "macos"))]
    start_rdev(app);
}

// ─── macOS：CGEventTap 挂到主 RunLoop，非阻塞 ─────────────────────────────────

#[cfg(target_os = "macos")]
fn start_macos(app: AppHandle) {
    use std::ffi::c_void;

    APP_HANDLE.set(app).ok();

    const KCG_SESSION_EVENT_TAP: u32 = 1;      // 会话级，只需辅助功能权限
    const KCG_TAIL_APPEND_EVENT_TAP: u32 = 1;  // 被动监听用 tail
    const KCG_EVENT_TAP_OPTION_LISTEN_ONLY: u32 = 1;
    const KCG_EVENT_MASK_KEY_DOWN: u64 = 1 << 10;

    #[link(name = "ApplicationServices", kind = "framework")]
    extern "C" {
        fn CGEventTapCreate(
            tap: u32,
            place: u32,
            options: u32,
            events_of_interest: u64,
            callback: extern "C" fn(*mut c_void, u32, *mut c_void, *mut c_void) -> *mut c_void,
            user_info: *mut c_void,
        ) -> *mut c_void;
        fn CGEventTapEnable(tap: *mut c_void, enable: bool);
        fn AXIsProcessTrusted() -> bool;
    }

    #[link(name = "CoreFoundation", kind = "framework")]
    extern "C" {
        fn CFMachPortCreateRunLoopSource(
            allocator: *const c_void,
            port: *mut c_void,
            order: isize,
        ) -> *mut c_void;
        fn CFRunLoopGetMain() -> *mut c_void;
        fn CFRunLoopAddSource(rl: *mut c_void, source: *mut c_void, mode: *const c_void);
        static kCFRunLoopCommonModes: *const c_void;
    }

    // C 函数指针作为回调（不能捕获，通过全局 APP_HANDLE 访问）
    extern "C" fn on_key_event(
        _proxy: *mut c_void,
        event_type: u32,
        _event: *mut c_void,
        _user_info: *mut c_void,
    ) -> *mut c_void {
        const KEY_DOWN: u32 = 10;
        if event_type == KEY_DOWN {
            let count = KEY_COUNT.fetch_add(1, Ordering::Relaxed) + 1;
            println!("[key] total={}", count);
            if let Some(app) = APP_HANDLE.get() {
                let _ = app.emit("key-press", count);
            }
        }
        std::ptr::null_mut()
    }

    unsafe {
        let trusted = AXIsProcessTrusted();
        println!("[key-listener] 辅助功能权限: {}", if trusted { "已授权 ✓" } else { "未授权 ✗" });

        if !trusted {
            println!("[key-listener] 请前往「系统设置 > 隐私与安全 > 辅助功能」授权");
            return;
        }

        let tap = CGEventTapCreate(
            KCG_SESSION_EVENT_TAP,
            KCG_TAIL_APPEND_EVENT_TAP,
            KCG_EVENT_TAP_OPTION_LISTEN_ONLY,
            KCG_EVENT_MASK_KEY_DOWN,
            on_key_event,
            std::ptr::null_mut(),
        );

        if tap.is_null() {
            println!("[key-listener] CGEventTapCreate 失败 — 即使已授权也失败，请重启应用");
            return;
        }

        let source = CFMachPortCreateRunLoopSource(std::ptr::null(), tap, 0);
        if source.is_null() {
            println!("[key-listener] CFMachPortCreateRunLoopSource 失败");
            return;
        }

        // 关键：挂到主 RunLoop，由 Tauri 的事件循环驱动，不需要额外线程
        let main_loop = CFRunLoopGetMain();
        CFRunLoopAddSource(main_loop, source, kCFRunLoopCommonModes);
        CGEventTapEnable(tap, true);

        println!("[key-listener] CGEventTap 已挂载到主 RunLoop ✓ 开始监听按键");
    }
}

// ─── Windows / Linux：rdev 后台线程 ───────────────────────────────────────────

#[cfg(not(target_os = "macos"))]
fn start_rdev(app: AppHandle) {
    use rdev::{listen, Event, EventType};
    use std::sync::Arc;

    std::thread::Builder::new()
        .name("key-listener".into())
        .spawn(move || {
            let app = Arc::new(app);
            println!("[key-listener] 监听线程已启动");
            let result = listen(move |event: Event| {
                if let EventType::KeyPress(key) = event.event_type {
                    let count = KEY_COUNT.fetch_add(1, Ordering::Relaxed) + 1;
                    println!("[key] {:?}  total={}", key, count);
                    let _ = app.emit("key-press", count);
                }
            });
            if let Err(e) = result {
                println!("[key-listener] 监听失败: {:?}", e);
            }
        })
        .expect("failed to spawn key-listener thread");
}
