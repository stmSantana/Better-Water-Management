﻿using HarmonyLib;
using MelonLoader.TinyJSON;
using System;
using System.Reflection;
using UnhollowerBaseLib.Attributes;

namespace BetterWaterManagement
{
	//this is to prevent the issue where saving a game and reloading would cause
	//cooking pots with boiled water to become non-potable.
	public class CookingPotWaterSaveData : CustomSaveDataUtilities.ModSaveBehaviour
	{
		public CookingPotItem.CookingState cookingState;
		public float litersSnowBeingMelted;
		public float litersWaterBeingBoiled;
		public float minutesUntilCooked;
		public float minutesUntilRuined;

		public CookingPotWaterSaveData(System.IntPtr intPtr) : base(intPtr) { }

		[HideFromIl2Cpp]
		public override void Deserialize(string data)
		{
			//Implementation.Log("Deserializing CookingPotWaterSaveData");
			if (!string.IsNullOrEmpty(data))
			{
				CookingPotWaterSaveProxy saveProxy = JSON.Load(data).Make<CookingPotWaterSaveProxy>();

				this.cookingState = saveProxy.cookingState;
				this.litersSnowBeingMelted = saveProxy.litersSnowBeingMelted;
				this.litersWaterBeingBoiled = saveProxy.litersWaterBeingBoiled;
				this.minutesUntilCooked = saveProxy.minutesUntilCooked;
				this.minutesUntilRuined = saveProxy.minutesUntilRuined;
				ApplyToCookingPot();
			}
		}

		[HideFromIl2Cpp]
		public override string Serialize()
		{
			//Implementation.Log("Serializing CookingPotWaterSaveData");
			GetFromCookingPot();
			CookingPotWaterSaveProxy saveProxy = new CookingPotWaterSaveProxy
			{
				cookingState = this.cookingState,
				litersSnowBeingMelted = this.litersSnowBeingMelted,
				litersWaterBeingBoiled = this.litersWaterBeingBoiled,
				minutesUntilCooked = this.minutesUntilCooked,
				minutesUntilRuined = this.minutesUntilRuined
			};

			return JSON.Dump(saveProxy);
		}

		internal void ApplyToCookingPot()
		{
			CookingPotItem cookingPot = this.GetComponent<CookingPotItem>();
			if (cookingPot == null)
			{
				MelonLoader.MelonLogger.Error("CookingPotWaterSaveData applied to a non-cookingpotitem!");
			}
			else
			{
				cookingPot.m_CookingState = this.cookingState;
				cookingPot.m_LitersSnowBeingMelted = this.litersSnowBeingMelted;
				cookingPot.m_LitersWaterBeingBoiled = this.litersWaterBeingBoiled;
				cookingPot.m_MinutesUntilCooked = this.minutesUntilCooked;
				cookingPot.m_MinutesUntilRuined = this.minutesUntilRuined;
				if (cookingState == CookingPotItem.CookingState.Cooking)
				{
					cookingPot.m_GrubMeshRenderer.sharedMaterials = cookingPot.m_BoilWaterPotMaterialsList;
				}
				else if (cookingState == CookingPotItem.CookingState.Ready)
				{
					cookingPot.m_GrubMeshRenderer.sharedMaterials = cookingPot.m_BoilWaterReadyMaterialsList;
				}
			}
		}

		internal void GetFromCookingPot()
		{
			CookingPotItem cookingPot = this.GetComponent<CookingPotItem>();
			if (cookingPot == null)
			{
				MelonLoader.MelonLogger.Error("CookingPotWaterSaveData trying to get data from a non-cookingpotitem!");
			}
			else
			{
				this.cookingState = cookingPot.m_CookingState;
				this.litersSnowBeingMelted = cookingPot.m_LitersSnowBeingMelted;
				this.litersWaterBeingBoiled = cookingPot.m_LitersWaterBeingBoiled;
				this.minutesUntilCooked = cookingPot.m_MinutesUntilCooked;
				this.minutesUntilRuined = cookingPot.m_MinutesUntilRuined;
			}
		}

		private static void CopyFields<T>(T copyTo, T copyFrom)
		{
			Type typeOfT = typeof(T);
			FieldInfo[] fieldInfos = typeOfT.GetFields();
			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				fieldInfo.SetValue(copyTo, fieldInfo.GetValue(copyFrom));
			}
			if (fieldInfos.Length == 0)
			{
				MelonLoader.MelonLogger.Error("There were no fields to copy!");
			}
		}
	}

	public struct CookingPotWaterSaveProxy
	{
		public CookingPotItem.CookingState cookingState;
		public float litersSnowBeingMelted;
		public float litersWaterBeingBoiled;
		public float minutesUntilCooked;
		public float minutesUntilRuined;
	}

	internal class CookingPotWaterPatches
	{
		[HarmonyPatch(typeof(GearItem), "Serialize")]
		internal class GearItem_Serialize
		{
			[HarmonyPriority(Priority.First)]
			private static void Prefix(GearItem __instance)
			{
				CookingPotItem cookingPot = __instance.m_CookingPotItem;
				if (cookingPot != null)
				{
					ModComponent.Utils.ComponentUtils.GetOrCreateComponent<CookingPotWaterSaveData>(__instance).GetFromCookingPot();
				}
			}
		}
	}
}
