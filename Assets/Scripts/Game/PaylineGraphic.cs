using UnityEngine;
using UnityEngine.UI;
using Slot.Core;

namespace Slot.Game
{
    // Shared by the actual reel overlay and the forty small rule diagrams.
    public class PaylineGraphic : MaskableGraphic
    {
        [SerializeField] private int lineId = 1;
        [SerializeField] private int matched = 5;
        [SerializeField] private bool visible = true;
        [SerializeField] private bool reelOverlay;
        [SerializeField] private bool outlineMatches;

        public void Configure(int id, bool overlay) { lineId = id; reelOverlay = overlay; SetVerticesDirty(); }
        public void Show(int id, int count, bool show, bool outlines, Color tint)
        { lineId = id; matched = count; visible = show; outlineMatches = outlines; color = tint; SetVerticesDirty(); }

        private Vector2 Point(int cell)
        {
            int col = (cell-1)/4, row = (cell-1)%4;
            if (reelOverlay) return new Vector2(-208 + col*104, ReelColumn.SlotY[row]);
            var r = rectTransform.rect;
            return new Vector2(r.xMin+8+col*(r.width-16)/4, r.yMax-7-row*(r.height-14)/3);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (!visible || lineId < 1 || lineId > SlotPaylines.All.Length) return;
            var line = SlotPaylines.All[lineId-1];
            if (!reelOverlay)
                for (int col=0; col<5; col++) for (int row=0; row<4; row++)
                {
                    var p=Point(col*4+row+1);
                    Quad(vh,p+new Vector2(-1.5f,-1.5f),p+new Vector2(1.5f,1.5f),new Color(.5f,.6f,.7f,.35f));
                }
            int count = Mathf.Clamp(matched,1,5);
            // One dark outer pass keeps the thicker path readable over bright fruit.
            // The small rule diagrams use a plain stroke to retain their spacing.
            if(reelOverlay)
            {
                DrawPath(vh,line.Cells,count,7,6,new Color(.06f,.045f,.08f,.72f));
                var ink=color;ink.a=1;
                DrawPath(vh,line.Cells,count,5,4,ink);
            }
            else DrawPath(vh,line.Cells,count,3,0,color);
        }
        private void DrawPath(VertexHelper vh,int[] cells,int count,float lineWidth,float frameWidth,Color tint)
        {
            for(int i=1;i<count;i++)Segment(vh,Point(cells[i-1]),Point(cells[i]),lineWidth,tint);
            if(outlineMatches&&reelOverlay)
                for(int i=0;i<count;i++)
                {
                    var p=Point(cells[i]);var w=ReelColumn.CellW/2-3;var h=ReelColumn.CellH/2-3;
                    Segment(vh,p+new Vector2(-w,-h),p+new Vector2(w,-h),frameWidth,tint);
                    Segment(vh,p+new Vector2(w,-h),p+new Vector2(w,h),frameWidth,tint);
                    Segment(vh,p+new Vector2(w,h),p+new Vector2(-w,h),frameWidth,tint);
                    Segment(vh,p+new Vector2(-w,h),p+new Vector2(-w,-h),frameWidth,tint);
                }
        }
        private static void Segment(VertexHelper vh,Vector2 a,Vector2 b,float width,Color tint)
        {
            var n=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;int index=vh.currentVertCount;
            vh.AddVert(a-n,tint,Vector2.zero);vh.AddVert(a+n,tint,Vector2.zero);
            vh.AddVert(b+n,tint,Vector2.zero);vh.AddVert(b-n,tint,Vector2.zero);
            vh.AddTriangle(index,index+1,index+2);vh.AddTriangle(index,index+2,index+3);
        }
        private static void Quad(VertexHelper vh,Vector2 min,Vector2 max,Color tint)
        {
            int i=vh.currentVertCount;
            vh.AddVert(min,tint,Vector2.zero);vh.AddVert(new Vector2(min.x,max.y),tint,Vector2.zero);
            vh.AddVert(max,tint,Vector2.zero);vh.AddVert(new Vector2(max.x,min.y),tint,Vector2.zero);
            vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
        }
    }
}
