using System.Collections;
using System.Linq;
using NUnit.Framework;
using Slot.Game;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Slot.Tests
{
    public class ReelBrakeBoundaryTests
    {
        private GameObject root;
        [UnityTearDown] public IEnumerator Cleanup()
        {Object.Destroy(root); yield return null;}

        [UnityTest] public IEnumerator HalfCellAndNearGridStopsKeepEveryIconDistinct()
        {
            root=new GameObject("brake boundary",typeof(RectTransform));
            var reel=root.AddComponent<ReelColumn>();
            reel.Init(root.transform);
            reel.SetRandomAll(new System.Random(42));
            // Half-cell ties previously rounded neighboring icons in opposite directions.
            float[] offsets={46f,-46f,45.9999f,46.0001f,-45.9999f,-46.0001f,
                0f,1.9999f,2f,2.0001f,-1.9999f,-2f,-2.0001f,91.999f,-91.999f,138f};
            foreach(float offset in offsets)
            {
                reel.StartScroll();
                for(int i=0;i<6;i++)
                {
                    float y=230-i*92+offset;
                    while(y < -276)y+=552;
                    while(y >= 276)y-=552;
                    ((RectTransform)root.transform.Find("icon"+i)).anchoredPosition=new Vector2(0,y);
                }
                var images=root.GetComponentsInChildren<Image>();
                var sprites=images.Select(i=>i.sprite).ToArray();
                reel.BeginBrake();
                float deadline=Time.realtimeSinceStartup+2;
                while(!reel.IsIdle&&Time.realtimeSinceStartup<deadline)yield return null;
                Assert.IsTrue(reel.IsIdle,$"Stop timed out at offset {offset}");
                var symbols=reel.GetVisibleSymbols();
                Assert.AreEqual(4,symbols.Length,$"offset {offset}");
                for(int row=0;row<4;row++)
                {
                    Assert.AreEqual(1,images.Count(i=>Mathf.Abs(i.rectTransform.anchoredPosition.y-ReelColumn.SlotY[row])<.1f),$"Cell {row}, offset {offset}");
                    Assert.AreEqual(ReelCell.GetFruitSprite(symbols[row]),reel.GetVisibleIcon(row).sprite);
                }
                var positions=images.Select(i=>i.rectTransform.anchoredPosition.y).OrderBy(y=>y).ToArray();
                for(int i=1;i<6;i++)Assert.AreEqual(92,positions[i]-positions[i-1],.01f,$"Strip gap, offset {offset}");
                CollectionAssert.AreEqual(sprites,images.Select(i=>i.sprite).ToArray(),"Braking must not replace symbols.");
            }
        }
    }
}
