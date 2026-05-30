using System.Text;
using Steamworks;
using UnityEngine;

public class SteamInventoryTest : MonoBehaviour
{
	public static class ESpaceWarItemDefIDs
	{
		public static readonly SteamItemDef_t k_SpaceWarItem_TimedDropList = (SteamItemDef_t)10;

		public static readonly SteamItemDef_t k_SpaceWarItem_ShipDecoration1 = (SteamItemDef_t)100;

		public static readonly SteamItemDef_t k_SpaceWarItem_ShipDecoration2 = (SteamItemDef_t)101;

		public static readonly SteamItemDef_t k_SpaceWarItem_ShipDecoration3 = (SteamItemDef_t)102;

		public static readonly SteamItemDef_t k_SpaceWarItem_ShipDecoration4 = (SteamItemDef_t)103;
	}

	private Vector2 m_ScrollPos;

	private SteamInventoryResult_t m_SteamInventoryResult;

	private SteamItemDetails_t[] m_SteamItemDetails;

	private SteamItemDef_t[] m_SteamItemDef;

	private byte[] m_SerializedBuffer;

	protected Callback<SteamInventoryResultReady_t> m_SteamInventoryResultReady;

	protected Callback<SteamInventoryFullUpdate_t> m_SteamInventoryFullUpdate;

	protected Callback<SteamInventoryDefinitionUpdate_t> m_SteamInventoryDefinitionUpdate;

	protected Callback<SteamInventoryRequestPricesResult_t> m_SteamInventoryRequestPricesResult;

	private CallResult<SteamInventoryEligiblePromoItemDefIDs_t> OnSteamInventoryEligiblePromoItemDefIDsCallResult;

	private CallResult<SteamInventoryStartPurchaseResult_t> OnSteamInventoryStartPurchaseResultCallResult;

	public void OnEnable()
	{
		m_SteamInventoryResult = SteamInventoryResult_t.Invalid;
		m_SteamItemDetails = null;
		m_SteamItemDef = null;
		m_SerializedBuffer = null;
		m_SteamInventoryResultReady = Callback<SteamInventoryResultReady_t>.Create(OnSteamInventoryResultReady);
		m_SteamInventoryFullUpdate = Callback<SteamInventoryFullUpdate_t>.Create(OnSteamInventoryFullUpdate);
		m_SteamInventoryDefinitionUpdate = Callback<SteamInventoryDefinitionUpdate_t>.Create(OnSteamInventoryDefinitionUpdate);
		m_SteamInventoryRequestPricesResult = Callback<SteamInventoryRequestPricesResult_t>.Create(OnSteamInventoryRequestPricesResult);
		OnSteamInventoryEligiblePromoItemDefIDsCallResult = CallResult<SteamInventoryEligiblePromoItemDefIDs_t>.Create(OnSteamInventoryEligiblePromoItemDefIDs);
		OnSteamInventoryStartPurchaseResultCallResult = CallResult<SteamInventoryStartPurchaseResult_t>.Create(OnSteamInventoryStartPurchaseResult);
	}

	public void OnDisable()
	{
		DestroyResult();
	}

	private void DestroyResult()
	{
		if (m_SteamInventoryResult != SteamInventoryResult_t.Invalid)
		{
			SteamInventory.DestroyResult(m_SteamInventoryResult);
			SteamInventoryResult_t steamInventoryResult = m_SteamInventoryResult;
			MonoBehaviour.print("SteamInventory.DestroyResult(" + steamInventoryResult.ToString() + ")");
			m_SteamInventoryResult = SteamInventoryResult_t.Invalid;
		}
	}

	public void OnGUI()
	{
		GUILayout.BeginArea(new Rect(Screen.width - 200, 0f, 200f, Screen.height));
		GUILayout.Label("Variables:");
		SteamInventoryResult_t steamInventoryResult = m_SteamInventoryResult;
		GUILayout.Label("m_SteamInventoryResult: " + steamInventoryResult.ToString());
		GUILayout.Label("m_SteamItemDetails: " + m_SteamItemDetails);
		GUILayout.Label("m_SteamItemDef: " + m_SteamItemDef);
		GUILayout.Label("m_SerializedBuffer: " + m_SerializedBuffer);
		GUILayout.EndArea();
		GUILayout.BeginVertical("box");
		m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos, GUILayout.Width(Screen.width - 215), GUILayout.Height(Screen.height - 33));
		GUILayout.Label("GetResultStatus(m_SteamInventoryResult) : " + SteamInventory.GetResultStatus(m_SteamInventoryResult));
		if (GUILayout.Button("GetResultItems(m_SteamInventoryResult, m_SteamItemDetails, ref OutItemsArraySize)"))
		{
			uint punOutItemsArraySize = 0u;
			bool resultItems = SteamInventory.GetResultItems(m_SteamInventoryResult, null, ref punOutItemsArraySize);
			if (resultItems && punOutItemsArraySize != 0)
			{
				m_SteamItemDetails = new SteamItemDetails_t[punOutItemsArraySize];
				resultItems = SteamInventory.GetResultItems(m_SteamInventoryResult, m_SteamItemDetails, ref punOutItemsArraySize);
				string[] obj = new string[6] { "SteamInventory.GetResultItems(", null, null, null, null, null };
				steamInventoryResult = m_SteamInventoryResult;
				obj[1] = steamInventoryResult.ToString();
				obj[2] = ", m_SteamItemDetails, out OutItemsArraySize) - ";
				obj[3] = resultItems.ToString();
				obj[4] = " -- ";
				obj[5] = punOutItemsArraySize.ToString();
				MonoBehaviour.print(string.Concat(obj));
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < punOutItemsArraySize; i++)
				{
					stringBuilder.AppendFormat("{0} - {1} - {2} - {3} - {4}\n", i, m_SteamItemDetails[i].m_itemId, m_SteamItemDetails[i].m_iDefinition, m_SteamItemDetails[i].m_unQuantity, m_SteamItemDetails[i].m_unFlags);
				}
				MonoBehaviour.print(stringBuilder);
			}
			else
			{
				string[] obj2 = new string[6] { "SteamInventory.GetResultItems(", null, null, null, null, null };
				steamInventoryResult = m_SteamInventoryResult;
				obj2[1] = steamInventoryResult.ToString();
				obj2[2] = ", null, out OutItemsArraySize) - ";
				obj2[3] = resultItems.ToString();
				obj2[4] = " -- ";
				obj2[5] = punOutItemsArraySize.ToString();
				MonoBehaviour.print(string.Concat(obj2));
			}
		}
		if (GUILayout.Button("GetResultItemProperty(m_SteamInventoryResult, 0, null, out ValueBuffer, ref ValueBufferSize)"))
		{
			uint punValueBufferSizeOut = 0u;
			string pchValueBuffer;
			bool resultItemProperty = SteamInventory.GetResultItemProperty(m_SteamInventoryResult, 0u, null, out pchValueBuffer, ref punValueBufferSizeOut);
			if (resultItemProperty)
			{
				resultItemProperty = SteamInventory.GetResultItemProperty(m_SteamInventoryResult, 0u, null, out pchValueBuffer, ref punValueBufferSizeOut);
			}
			string[] obj3 = new string[10] { "SteamInventory.GetResultItemProperty(", null, null, null, null, null, null, null, null, null };
			steamInventoryResult = m_SteamInventoryResult;
			obj3[1] = steamInventoryResult.ToString();
			obj3[2] = ", ";
			obj3[3] = 0.ToString();
			obj3[4] = ", , out ValueBuffer, ref ValueBufferSize) : ";
			obj3[5] = resultItemProperty.ToString();
			obj3[6] = " -- ";
			obj3[7] = pchValueBuffer;
			obj3[8] = " -- ";
			obj3[9] = punValueBufferSizeOut.ToString();
			MonoBehaviour.print(string.Concat(obj3));
		}
		if (GUILayout.Button("GetResultTimestamp(m_SteamInventoryResult)"))
		{
			uint resultTimestamp = SteamInventory.GetResultTimestamp(m_SteamInventoryResult);
			steamInventoryResult = m_SteamInventoryResult;
			MonoBehaviour.print("SteamInventory.GetResultTimestamp(" + steamInventoryResult.ToString() + ") : " + resultTimestamp);
		}
		if (GUILayout.Button("CheckResultSteamID(m_SteamInventoryResult, SteamUser.GetSteamID())"))
		{
			bool flag = SteamInventory.CheckResultSteamID(m_SteamInventoryResult, SteamUser.GetSteamID());
			string[] obj4 = new string[6] { "SteamInventory.CheckResultSteamID(", null, null, null, null, null };
			steamInventoryResult = m_SteamInventoryResult;
			obj4[1] = steamInventoryResult.ToString();
			obj4[2] = ", ";
			obj4[3] = SteamUser.GetSteamID().ToString();
			obj4[4] = ") : ";
			obj4[5] = flag.ToString();
			MonoBehaviour.print(string.Concat(obj4));
		}
		if (GUILayout.Button("DestroyResult(m_SteamInventoryResult)"))
		{
			DestroyResult();
		}
		if (GUILayout.Button("GetAllItems(out m_SteamInventoryResult)"))
		{
			bool allItems = SteamInventory.GetAllItems(out m_SteamInventoryResult);
			string text = allItems.ToString();
			steamInventoryResult = m_SteamInventoryResult;
			MonoBehaviour.print("SteamInventory.GetAllItems(out m_SteamInventoryResult) : " + text + " -- " + steamInventoryResult.ToString());
		}
		if (GUILayout.Button("GetItemsByID(out m_SteamInventoryResult, InstanceIDs, (uint)InstanceIDs.Length)"))
		{
			SteamItemInstanceID_t[] array = new SteamItemInstanceID_t[2]
			{
				(SteamItemInstanceID_t)0uL,
				(SteamItemInstanceID_t)1uL
			};
			bool itemsByID = SteamInventory.GetItemsByID(out m_SteamInventoryResult, array, (uint)array.Length);
			string[] obj5 = new string[8]
			{
				"SteamInventory.GetItemsByID(out m_SteamInventoryResult, ",
				array?.ToString(),
				", ",
				((uint)array.Length).ToString(),
				") : ",
				itemsByID.ToString(),
				" -- ",
				null
			};
			steamInventoryResult = m_SteamInventoryResult;
			obj5[7] = steamInventoryResult.ToString();
			MonoBehaviour.print(string.Concat(obj5));
		}
		if (GUILayout.Button("SerializeResult(m_SteamInventoryResult, m_SerializedBuffer, out OutBufferSize)"))
		{
			bool flag2 = SteamInventory.SerializeResult(m_SteamInventoryResult, null, out var punOutBufferSize);
			if (flag2)
			{
				m_SerializedBuffer = new byte[punOutBufferSize];
				flag2 = SteamInventory.SerializeResult(m_SteamInventoryResult, m_SerializedBuffer, out punOutBufferSize);
				MonoBehaviour.print("SteamInventory.SerializeResult(m_SteamInventoryResult, m_SerializedBuffer, out OutBufferSize) - " + flag2 + " -- " + punOutBufferSize + " -- " + Encoding.UTF8.GetString(m_SerializedBuffer, 0, m_SerializedBuffer.Length));
			}
			else
			{
				MonoBehaviour.print("SteamInventory.SerializeResult(m_SteamInventoryResult, null, out OutBufferSize) - " + flag2 + " -- " + punOutBufferSize);
			}
		}
		if (GUILayout.Button("DeserializeResult(out m_SteamInventoryResult, m_SerializedBuffer, (uint)m_SerializedBuffer.Length)"))
		{
			bool flag3 = SteamInventory.DeserializeResult(out m_SteamInventoryResult, m_SerializedBuffer, (uint)m_SerializedBuffer.Length);
			string[] obj6 = new string[8]
			{
				"SteamInventory.DeserializeResult(out m_SteamInventoryResult, ",
				m_SerializedBuffer?.ToString(),
				", ",
				((uint)m_SerializedBuffer.Length).ToString(),
				") : ",
				flag3.ToString(),
				" -- ",
				null
			};
			steamInventoryResult = m_SteamInventoryResult;
			obj6[7] = steamInventoryResult.ToString();
			MonoBehaviour.print(string.Concat(obj6));
		}
		if (GUILayout.Button("GenerateItems(out m_SteamInventoryResult, ArrayItemDefs, null, (uint)ArrayItemDefs.Length)"))
		{
			SteamItemDef_t[] array2 = new SteamItemDef_t[2]
			{
				ESpaceWarItemDefIDs.k_SpaceWarItem_ShipDecoration1,
				ESpaceWarItemDefIDs.k_SpaceWarItem_ShipDecoration2
			};
			bool flag4 = SteamInventory.GenerateItems(out m_SteamInventoryResult, array2, null, (uint)array2.Length);
			string[] obj7 = new string[8]
			{
				"SteamInventory.GenerateItems(out m_SteamInventoryResult, ",
				array2?.ToString(),
				", , ",
				((uint)array2.Length).ToString(),
				") : ",
				flag4.ToString(),
				" -- ",
				null
			};
			steamInventoryResult = m_SteamInventoryResult;
			obj7[7] = steamInventoryResult.ToString();
			MonoBehaviour.print(string.Concat(obj7));
		}
		if (GUILayout.Button("GrantPromoItems(out m_SteamInventoryResult)"))
		{
			bool flag5 = SteamInventory.GrantPromoItems(out m_SteamInventoryResult);
			string text2 = flag5.ToString();
			steamInventoryResult = m_SteamInventoryResult;
			MonoBehaviour.print("SteamInventory.GrantPromoItems(out m_SteamInventoryResult) : " + text2 + " -- " + steamInventoryResult.ToString());
		}
		if (GUILayout.Button("ConsumeItem(out m_SteamInventoryResult, m_SteamItemDetails[0].m_itemId, 1)") && m_SteamItemDetails != null)
		{
			bool flag6 = SteamInventory.ConsumeItem(out m_SteamInventoryResult, m_SteamItemDetails[0].m_itemId, 1u);
			string[] obj8 = new string[6] { "SteamInventory.ConsumeItem(out m_SteamInventoryResult, ", null, null, null, null, null };
			SteamItemInstanceID_t itemId = m_SteamItemDetails[0].m_itemId;
			obj8[1] = itemId.ToString();
			obj8[2] = ", 1) - ";
			obj8[3] = flag6.ToString();
			obj8[4] = " -- ";
			steamInventoryResult = m_SteamInventoryResult;
			obj8[5] = steamInventoryResult.ToString();
			MonoBehaviour.print(string.Concat(obj8));
		}
		if (GUILayout.Button("ExchangeItems(TODO)") && m_SteamItemDetails != null)
		{
			bool flag7 = SteamInventory.ExchangeItems(out m_SteamInventoryResult, null, null, 0u, null, null, 0u);
			string text3 = flag7.ToString();
			steamInventoryResult = m_SteamInventoryResult;
			MonoBehaviour.print("SteamInventory.ExchangeItems(TODO) - " + text3 + " -- " + steamInventoryResult.ToString());
		}
		if (GUILayout.Button("TransferItemQuantity(out m_SteamInventoryResult, m_SteamItemDetails[0].m_itemId, 1, SteamItemInstanceID_t.Invalid)") && m_SteamItemDetails != null)
		{
			bool flag8 = SteamInventory.TransferItemQuantity(out m_SteamInventoryResult, m_SteamItemDetails[0].m_itemId, 1u, SteamItemInstanceID_t.Invalid);
			string[] obj9 = new string[6] { "SteamInventory.TransferItemQuantity(out m_SteamInventoryResult, ", null, null, null, null, null };
			SteamItemInstanceID_t itemId = m_SteamItemDetails[0].m_itemId;
			obj9[1] = itemId.ToString();
			obj9[2] = ", 1, SteamItemInstanceID_t.Invalid) - ";
			obj9[3] = flag8.ToString();
			obj9[4] = " -- ";
			steamInventoryResult = m_SteamInventoryResult;
			obj9[5] = steamInventoryResult.ToString();
			MonoBehaviour.print(string.Concat(obj9));
		}
		if (GUILayout.Button("SendItemDropHeartbeat()"))
		{
			SteamInventory.SendItemDropHeartbeat();
			MonoBehaviour.print("SteamInventory.SendItemDropHeartbeat()");
		}
		if (GUILayout.Button("TriggerItemDrop(out m_SteamInventoryResult, ESpaceWarItemDefIDs.k_SpaceWarItem_TimedDropList)"))
		{
			bool flag9 = SteamInventory.TriggerItemDrop(out m_SteamInventoryResult, ESpaceWarItemDefIDs.k_SpaceWarItem_TimedDropList);
			string[] obj10 = new string[6]
			{
				"SteamInventory.TriggerItemDrop(out m_SteamInventoryResult, ",
				ESpaceWarItemDefIDs.k_SpaceWarItem_TimedDropList.ToString(),
				") : ",
				flag9.ToString(),
				" -- ",
				null
			};
			steamInventoryResult = m_SteamInventoryResult;
			obj10[5] = steamInventoryResult.ToString();
			MonoBehaviour.print(string.Concat(obj10));
		}
		if (GUILayout.Button("TradeItems(TODO)") && m_SteamItemDetails != null)
		{
			bool flag10 = SteamInventory.TradeItems(out m_SteamInventoryResult, SteamUser.GetSteamID(), null, null, 0u, null, null, 0u);
			string text4 = flag10.ToString();
			steamInventoryResult = m_SteamInventoryResult;
			MonoBehaviour.print("SteamInventory.TradeItems(TODO) - " + text4 + " -- " + steamInventoryResult.ToString());
		}
		if (GUILayout.Button("LoadItemDefinitions()"))
		{
			MonoBehaviour.print("SteamInventory.LoadItemDefinitions() : " + SteamInventory.LoadItemDefinitions());
		}
		if (GUILayout.Button("GetItemDefinitionIDs(ItemDefIDs, ref length)"))
		{
			uint punItemDefIDsArraySize = 0u;
			bool itemDefinitionIDs = SteamInventory.GetItemDefinitionIDs(null, ref punItemDefIDsArraySize);
			if (itemDefinitionIDs)
			{
				m_SteamItemDef = new SteamItemDef_t[punItemDefIDsArraySize];
				MonoBehaviour.print("SteamInventory.GetItemDefinitionIDs(m_SteamItemDef, ref length) - " + SteamInventory.GetItemDefinitionIDs(m_SteamItemDef, ref punItemDefIDsArraySize) + " -- " + punItemDefIDsArraySize);
			}
			else
			{
				MonoBehaviour.print("SteamInventory.GetItemDefinitionIDs(null, ref length) - " + itemDefinitionIDs + " -- " + punItemDefIDsArraySize);
			}
		}
		if (GUILayout.Button("GetItemDefinitionProperty(ESpaceWarItemDefIDs.k_SpaceWarItem_ShipDecoration1, null, out ValueBuffer, ref length)"))
		{
			uint punValueBufferSizeOut2 = 2048u;
			string pchValueBuffer2;
			bool itemDefinitionProperty = SteamInventory.GetItemDefinitionProperty(ESpaceWarItemDefIDs.k_SpaceWarItem_ShipDecoration1, null, out pchValueBuffer2, ref punValueBufferSizeOut2);
			MonoBehaviour.print("SteamInventory.GetItemDefinitionProperty(" + ESpaceWarItemDefIDs.k_SpaceWarItem_ShipDecoration1.ToString() + ", , out ValueBuffer, ref length) : " + itemDefinitionProperty + " -- " + pchValueBuffer2 + " -- " + punValueBufferSizeOut2);
		}
		if (GUILayout.Button("RequestEligiblePromoItemDefinitionsIDs(SteamUser.GetSteamID())"))
		{
			SteamAPICall_t steamAPICall_t = SteamInventory.RequestEligiblePromoItemDefinitionsIDs(SteamUser.GetSteamID());
			OnSteamInventoryEligiblePromoItemDefIDsCallResult.Set(steamAPICall_t);
			string text5 = SteamUser.GetSteamID().ToString();
			SteamAPICall_t steamAPICall_t2 = steamAPICall_t;
			MonoBehaviour.print("SteamInventory.RequestEligiblePromoItemDefinitionsIDs(" + text5 + ") : " + steamAPICall_t2.ToString());
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	private void OnSteamInventoryResultReady(SteamInventoryResultReady_t pCallback)
	{
		string[] obj = new string[6]
		{
			"[",
			4700.ToString(),
			" - SteamInventoryResultReady] - ",
			null,
			null,
			null
		};
		SteamInventoryResult_t handle = pCallback.m_handle;
		obj[3] = handle.ToString();
		obj[4] = " -- ";
		obj[5] = pCallback.m_result.ToString();
		Debug.Log(string.Concat(obj));
		m_SteamInventoryResult = pCallback.m_handle;
	}

	private void OnSteamInventoryFullUpdate(SteamInventoryFullUpdate_t pCallback)
	{
		string text = 4701.ToString();
		SteamInventoryResult_t handle = pCallback.m_handle;
		Debug.Log("[" + text + " - SteamInventoryFullUpdate] - " + handle.ToString());
		m_SteamInventoryResult = pCallback.m_handle;
	}

	private void OnSteamInventoryDefinitionUpdate(SteamInventoryDefinitionUpdate_t pCallback)
	{
		Debug.Log("[" + 4702 + " - SteamInventoryDefinitionUpdate]");
	}

	private void OnSteamInventoryEligiblePromoItemDefIDs(SteamInventoryEligiblePromoItemDefIDs_t pCallback, bool bIOFailure)
	{
		string[] obj = new string[10]
		{
			"[",
			4703.ToString(),
			" - SteamInventoryEligiblePromoItemDefIDs] - ",
			pCallback.m_result.ToString(),
			" -- ",
			null,
			null,
			null,
			null,
			null
		};
		CSteamID steamID = pCallback.m_steamID;
		obj[5] = steamID.ToString();
		obj[6] = " -- ";
		obj[7] = pCallback.m_numEligiblePromoItemDefs.ToString();
		obj[8] = " -- ";
		obj[9] = pCallback.m_bCachedData.ToString();
		Debug.Log(string.Concat(obj));
		uint punItemDefIDsArraySize = (uint)pCallback.m_numEligiblePromoItemDefs;
		SteamItemDef_t[] pItemDefIDs = new SteamItemDef_t[punItemDefIDsArraySize];
		MonoBehaviour.print("SteamInventory.GetEligiblePromoItemDefinitionIDs(pCallback.m_steamID, ItemDefIDs, ref ItemDefIDsArraySize) - " + SteamInventory.GetEligiblePromoItemDefinitionIDs(pCallback.m_steamID, pItemDefIDs, ref punItemDefIDsArraySize) + " -- " + punItemDefIDsArraySize);
	}

	private void OnSteamInventoryStartPurchaseResult(SteamInventoryStartPurchaseResult_t pCallback, bool bIOFailure)
	{
		Debug.Log("[" + 4704 + " - SteamInventoryStartPurchaseResult] - " + pCallback.m_result.ToString() + " -- " + pCallback.m_ulOrderID + " -- " + pCallback.m_ulTransID);
	}

	private void OnSteamInventoryRequestPricesResult(SteamInventoryRequestPricesResult_t pCallback)
	{
		Debug.Log("[" + 4705 + " - SteamInventoryRequestPricesResult] - " + pCallback.m_result.ToString() + " -- " + pCallback.m_rgchCurrency);
	}
}
