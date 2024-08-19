using System.Collections.Generic;
using Castle;
using Castle.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TinyGame
{
    public class PersonSpawn : WorldSpawn<PersonObject>
    {
        public SpriteRenderer spriteRenderer;
        private static readonly int FlashColorID = Shader.PropertyToID("_FlashColor");
        private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
        public Color FlashColor
        {
            get
            {
                var c = Color.white;
                spriteRenderer.SetProperty(x =>
                {
                    c = x.GetColor(FlashColorID);
                });
                return c;
            }
            set => spriteRenderer.SetProperty(block => block.SetColor(FlashColorID,value));
        }
        public Color OutlineColor
        {
            get
            {
                var c = Color.white;
                spriteRenderer.SetProperty(x =>
                {
                    c = x.GetColor(OutlineColorID);
                });
                return c;
            }
            set => spriteRenderer.SetProperty(block => block.SetColor(OutlineColorID,value));
        }
        // public override void UpdateSprites()
        // {
        //     base.UpdateSprites();
        //     transform.position = worldObject.virtualPosition;
        // }
        public override void TempSelect(bool isOn)
        {
            base.TempSelect(isOn);
            OutlineColor = isOn ? Color.white : Color.clear;
        }

        public override void Command(CastleGrid grid) => worldObject.Command(grid);
    }
}