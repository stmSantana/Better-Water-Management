﻿using Harmony;
using UnityEngine;

namespace BetterWaterManagement
{
    public class CoolDown : MonoBehaviour
    {
        private CookingPotItem cookingPotItem;
        private float elapsed;
        private float lastUpdate;
        private CookingPotItem.CookingState nextState;
        private CookingPotItem.CookingState originalState;

        public void FixedUpdate()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            elapsed += Time.fixedDeltaTime;
            if (elapsed - lastUpdate > 5)
            {
                lastUpdate = elapsed;
            }

            if (elapsed < 20)
            {
                return;
            }

            if (nextState < CookingPotItem.CookingState.Cooking)
            {
                AccessTools.Method(typeof(CookingPotItem), "TurnOnParticles").Invoke(this.cookingPotItem, new object[] { null });
                this.enabled = false;
                return;
            }

            System.Reflection.FieldInfo fieldInfo = AccessTools.Field(typeof(CookingPotItem), "m_CookingState");
            fieldInfo.SetValue(this.cookingPotItem, nextState);
            AccessTools.Method(typeof(CookingPotItem), "UpdateParticles").Invoke(this.cookingPotItem, null);
            fieldInfo.SetValue(this.cookingPotItem, originalState);
            nextState--;
            this.elapsed = 0;
        }

        public void SetEnabled(bool enable)
        {
            if (this.enabled == enable)
            {
                return;
            }

            if (enable)
            {
                this.enabled = true;
                this.elapsed = 0;
                this.lastUpdate = 0;
                this.originalState = this.cookingPotItem.GetCookingState();
                this.nextState = originalState - 1;
            }
            else
            {
                this.enabled = false;
            }
        }

        internal void Initialize()
        {
            if (this.cookingPotItem != null)
            {
                return;
            }

            this.cookingPotItem = this.GetComponent<CookingPotItem>();
            this.originalState = this.cookingPotItem.GetCookingState();
            this.nextState = originalState - 1;
        }

        public void Start()
        {
            this.Initialize();
        }
    }
}