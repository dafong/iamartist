namespace GlobalKeyHook
{
	public interface IGlobalKeyHook
	{
		bool Init();

		int ProcessInput(bool ignoreMouse);
	}
}
