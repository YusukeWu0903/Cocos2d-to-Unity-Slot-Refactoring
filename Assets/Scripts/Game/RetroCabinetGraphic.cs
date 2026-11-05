using UnityEngine;
using UnityEngine.UI;

namespace Slot.Game
{
    // Texture-free cabinet finishes: bevels, lens highlights and recessed reel paper.
    public class RetroCabinetGraphic : MaskableGraphic
    {
        public enum Finish { Purple, Gold, Chrome, Display, Paper, Button, Led, Screw }
        [SerializeField] private Finish finish;
        [SerializeField] private Color tone = new Color(.05f,.65f,.5f);
        public void Configure(Finish value,Color tint) { finish=value;tone=tint;SetVerticesDirty(); }
        public void SetTone(Color tint) { tone=tint;SetVerticesDirty(); }
        private static Color H(string value) { ColorUtility.TryParseHtmlString(value,out var c);return c; }
        private static Rect Inset(Rect r,float amount) => new Rect(r.x+amount,r.y+amount,r.width-amount*2,r.height-amount*2);
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;float radius=Mathf.Min(24,r.height*.5f);
            if(finish==Finish.Gold)
            {
                Round(vh,r,radius,H("#71350e"),H("#291108"));
                Round(vh,Inset(r,2),radius-2,H("#fff8c4"),H("#bd760f"));
                Round(vh,Inset(r,5),radius-5,H("#be6c0c"),H("#fff1a4"));
                Round(vh,Inset(r,8),radius-8,H("#fbd66b"),H("#b86613"));
            }
            else if(finish==Finish.Purple)
            {
                Round(vh,r,radius,H("#b652d6"),H("#1c063b"));
                Round(vh,Inset(r,4),radius-4,H("#75259e"),H("#300a57"));
                for(float y=r.yMin+20;y<r.yMax-16;y+=28)
                    for(float x=r.xMin+20;x<r.xMax-16;x+=28)
                        Diamond(vh,new Vector2(x,y),3,new Color(1,.7f,1,.07f));
            }
            else if(finish==Finish.Chrome)
            {
                Round(vh,r,16,H("#180b26"),Color.black);
                Round(vh,Inset(r,3),13,H("#f4dba7"),H("#7b6d5a"));
                Round(vh,Inset(r,7),10,H("#49454b"),H("#fff1d4"));
                Round(vh,Inset(r,10),7,H("#16131a"),H("#26232a"));
            }
            else if(finish==Finish.Display)
            {
                Round(vh,r,9,H("#e6bd62"),H("#5f3213"));
                Round(vh,Inset(r,2),7,H("#121815"),H("#020705"));
                for(float y=r.yMin+6;y<r.yMax-4;y+=6)
                    Round(vh,new Rect(r.xMin+5,y,r.width-10,1),0,new Color(.3f,1,.65f,.035f),new Color(.3f,1,.65f,.035f));
            }
            else if(finish==Finish.Paper)
            {
                Round(vh,r,0,H("#f8f6e8"),H("#e8eddf"));
                Round(vh,new Rect(r.xMin,r.yMax-32,r.width,32),0,H("#aaa99d"),new Color(1,1,.96f,0));
                Round(vh,new Rect(r.xMin,r.yMin,r.width,32),0,new Color(1,1,.96f,0),H("#969b8d"));
            }
            else if(finish==Finish.Button)
            {
                Round(vh,r,radius,H("#fff7b2"),H("#83400c"));
                Round(vh,Inset(r,3),radius-3,H("#642c0c"),H("#f9e492"));
                var inner=Inset(r,6);
                Round(vh,inner,radius-6,Color.Lerp(tone,Color.white,.4f),Color.Lerp(tone,Color.black,.45f));
                Round(vh,new Rect(inner.x+3,inner.center.y+1,inner.width-6,inner.height*.43f),Mathf.Min(16,inner.height*.2f),new Color(1,1,1,.46f),new Color(1,1,1,.02f));
            }
            else if(finish==Finish.Led)
            {
                Round(vh,r,r.height/2,new Color(tone.r,tone.g,tone.b,.18f),new Color(tone.r,tone.g,tone.b,.06f));
                Round(vh,Inset(r,3),r.height/2-3,Color.Lerp(tone,Color.white,.65f),tone);
                Round(vh,new Rect(r.center.x-r.width*.15f,r.center.y,r.width*.3f,r.height*.25f),r.height*.1f,new Color(1,1,1,.85f),new Color(1,1,1,0));
            }
            else
            {
                Round(vh,r,r.height/2,H("#fff2ae"),H("#7b4c18"));
                Round(vh,new Rect(r.xMin+3,r.center.y-1,r.width-6,2),0,H("#3a1c0c"),H("#3a1c0c"));
            }
        }
        private void Round(VertexHelper vh,Rect r,float radius,Color top,Color bottom)
        {
            if(r.width<=0||r.height<=0)return;
            radius=Mathf.Clamp(radius,0,Mathf.Min(r.width,r.height)*.5f);
            int start=vh.currentVertCount;vh.AddVert(r.center,Color.Lerp(bottom,top,.5f)*color,Vector2.zero);
            int total=28;
            for(int corner=0;corner<4;corner++)
            {
                var center=new Vector2(corner==0||corner==3?r.xMax-radius:r.xMin+radius,corner<2?r.yMax-radius:r.yMin+radius);
                for(int step=0;step<=6;step++)
                {
                    float angle=(corner*90+step*15)*Mathf.Deg2Rad;
                    var point=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
                    vh.AddVert(point,Color.Lerp(bottom,top,Mathf.InverseLerp(r.yMin,r.yMax,point.y))*color,Vector2.zero);
                }
            }
            for(int i=0;i<total;i++)vh.AddTriangle(start,start+1+i,start+1+(i+1)%total);
        }
        private void Diamond(VertexHelper vh,Vector2 p,float radius,Color tint)
        {
            int i=vh.currentVertCount;tint*=color;
            vh.AddVert(p+Vector2.up*radius,tint,Vector2.zero);vh.AddVert(p+Vector2.left*radius,tint,Vector2.zero);
            vh.AddVert(p+Vector2.down*radius,tint,Vector2.zero);vh.AddVert(p+Vector2.right*radius,tint,Vector2.zero);
            vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
        }
    }
}
