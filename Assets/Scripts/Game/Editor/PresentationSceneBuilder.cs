using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Slot.Game;

namespace Slot.EditorTools
{
    // A separate scene keeps the upgrade checkpoint available for comparison.
    public static class PresentationSceneBuilder
    {
        private static Font font;
        private static readonly Color Ink=new Color(.045f,.065f,.1f), Panel=new Color(.075f,.105f,.15f);
        private static readonly Color Muted=new Color(.62f,.7f,.78f), Mint=new Color(.42f,.91f,.82f);

        [MenuItem("Tools/Slot/Build Four Row Presentation")]
        public static void Build()
        {
            font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/SlotSansTC.ttf");
            if(font==null) throw new System.InvalidOperationException("Missing presentation font.");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag="MainCamera";camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Ink;
            camera.transform.position=new Vector3(0,0,-10);
            var canvas=new GameObject("Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1024,720);scaler.matchWidthOrHeight=1;
            var eventSystem=new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
            var controller=new GameObject("SlotRoot").AddComponent<SlotGameController>();
            var view=controller.gameObject.AddComponent<SlotPresentation>();
            var background=Image(canvas.transform,"Background",Vector2.zero,new Vector2(1024,720),Ink);Stretch(background.rectTransform);
            Text(canvas.transform,"Title","水果機重構展示",new Vector2(-245,308),new Vector2(430,48),30,Color.white,TextAnchor.MiddleLeft);
            Text(canvas.transform,"Subtitle","Cocos2d → Unity 6  /  四排固定支付線",new Vector2(-245,270),new Vector2(430,32),17,Muted,TextAnchor.MiddleLeft);
            var ruleButton=Button(canvas.transform,"Rules","規則與支付線",new Vector2(330,302),new Vector2(228,48));
            var reelArea=Image(canvas.transform,"ReelArea",new Vector2(-148,39),new Vector2(560,392),Panel);
            var mask=new GameObject("Mask",typeof(RectTransform),typeof(RectMask2D));mask.transform.SetParent(reelArea.transform,false);Stretch((RectTransform)mask.transform,6);
            var reels=new ReelColumn[5];
            for(int col=0;col<5;col++)
            {
                var column=Image(mask.transform,"Reel_"+col,new Vector2(-208+col*104,0),new Vector2(104,400),new Color(1,1,1,.015f));
                var reel=column.gameObject.AddComponent<ReelColumn>();reel.Init(column.transform);reels[col]=reel;
                foreach(Transform child in reel.transform)child.GetComponent<Image>().raycastTarget=false;
            }
            for(int row=0;row<4;row++)Text(canvas.transform,"Row_"+row,(row+1).ToString(),new Vector2(-455,39+ReelColumn.SlotY[row]),new Vector2(30,30),16,Muted);
            var pathGo=new GameObject("PaylineOverlay",typeof(RectTransform),typeof(PaylineGraphic));pathGo.transform.SetParent(reelArea.transform,false);
            var path=pathGo.GetComponent<PaylineGraphic>();path.rectTransform.sizeDelta=new Vector2(560,392);path.raycastTarget=false;path.Configure(1,true);path.Show(1,5,false,false,Mint);
            var status=Image(canvas.transform,"StatusPanel",new Vector2(313,39),new Vector2(296,392),Panel);
            Text(status.transform,"StatusLabel","本次結果",new Vector2(0,156),new Vector2(246,30),16,Muted,TextAnchor.MiddleLeft);
            var summary=Text(status.transform,"Summary","準備就緒",new Vector2(0,106),new Vector2(246,62),28,Mint,TextAnchor.MiddleLeft);
            var detail=Text(status.transform,"Detail","",new Vector2(0,2),new Vector2(246,156),18,Color.white,TextAnchor.UpperLeft);
            Text(status.transform,"DemoNote","互動原型 · 無實際金幣結算",new Vector2(0,-158),new Vector2(246,32),14,Muted,TextAnchor.MiddleLeft);
            var pathLabel=Text(canvas.transform,"PathLabel","已啟用 8 條線",new Vector2(-328,-190),new Vector2(244,36),17,Mint,TextAnchor.MiddleLeft);
            var toggle=Button(canvas.transform,"TogglePaths","查看路徑",new Vector2(-119,-190),new Vector2(108,38));
            var previous=Button(canvas.transform,"PreviousPath","‹",new Vector2(-33,-190),new Vector2(44,38));
            var next=Button(canvas.transform,"NextPath","›",new Vector2(20,-190),new Vector2(44,38));
            var bar=Image(canvas.transform,"ControlBar",new Vector2(0,-282),new Vector2(928,136),Panel);
            Text(bar.transform,"UnitLabel","押注單位",new Vector2(-356,39),new Vector2(180,28),16,Muted);
            Text(bar.transform,"LineLabel","啟用線數",new Vector2(-119,39),new Vector2(180,28),16,Muted);
            var unit=Text(bar.transform,"UnitValue","1",new Vector2(-356,-8),new Vector2(62,54),30,Color.white);
            var enabled=Text(bar.transform,"LineValue","8 / 40",new Vector2(-119,-8),new Vector2(116,54),26,Color.white);
            var mDn=Button(bar.transform,"MoneyDown","−",new Vector2(-422,-8),new Vector2(48,48));
            var mUp=Button(bar.transform,"MoneyUp","+",new Vector2(-290,-8),new Vector2(48,48));
            var bDn=Button(bar.transform,"BranchDown","−",new Vector2(-214,-8),new Vector2(48,48));
            var bUp=Button(bar.transform,"BranchUp","+",new Vector2(-24,-8),new Vector2(48,48));
            Text(bar.transform,"StopHint","轉動中再按 STOP\n可提早緩停",new Vector2(156,0),new Vector2(174,62),16,Muted);
            var spin=Button(bar.transform,"SpinButton","",new Vector2(361,0),new Vector2(124,120));
            var spinImage=spin.GetComponent<Image>();spinImage.sprite=Resources.Load<Sprite>("Sprites/ui/slot_BTN_unite_spin01");spinImage.color=Color.white;spinImage.preserveAspect=true;
            var overlay=Image(canvas.transform,"RulesOverlay",Vector2.zero,new Vector2(1024,720),new Color(0,0,0,.8f));Stretch(overlay.rectTransform);overlay.raycastTarget=true;
            var rules=Image(overlay.transform,"RulesPanel",Vector2.zero,new Vector2(940,652),new Color(.055f,.08f,.12f));rules.raycastTarget=true;
            Text(rules.transform,"RulesTitle","玩法與 40 條支付線",new Vector2(-140,285),new Vector2(610,46),28,Color.white,TextAnchor.MiddleLeft);
            var close=Button(rules.transform,"CloseRules","關閉",new Vector2(390,285),new Vector2(84,42));
            Text(rules.transform,"RulesBody","① 從最左軸沿啟用線，連續三個以上相符圖案即中獎。\n② 四排都參與；每檔至少含四條橫線。\n③ 鈴鐺可替代一般圖案，但不替代 GOOD LUCK／BAR。",new Vector2(-75,199),new Vector2(740,104),17,Color.white,TextAnchor.UpperLeft);
            for(int n=8;n<=10;n++)
            {
                var icon=Image(rules.transform,"RuleSymbol"+n,new Vector2(-385+(n-8)*85,104),new Vector2(54,54),Color.white);
                icon.sprite=Resources.Load<Sprite>("Sprites/fruits/icon_fruitbonus_"+n);icon.preserveAspect=true;
            }
            Text(rules.transform,"WildExamples","蘋果 → 鈴鐺 → 蘋果：算三連\nGOOD LUCK → 鈴鐺 → GOOD LUCK：不算三連\n全為鈴鐺可連線；GOOD LUCK／BAR 本身連續三個也可中獎。",new Vector2(135,103),new Vector2(550,76),15,Mint,TextAnchor.MiddleLeft);
            var choices=new Button[40];
            for(int i=0;i<40;i++)
            {
                choices[i]=Button(rules.transform,"LineChoice"+(i+1),"",new Vector2(-396+(i%10)*88,16-(i/10)*76),new Vector2(80,68));
                Text(choices[i].transform,"Number",(i+1).ToString("00"),new Vector2(0,21),new Vector2(64,20),13,Color.white);
                var diagram=new GameObject("Path",typeof(RectTransform),typeof(PaylineGraphic));diagram.transform.SetParent(choices[i].transform,false);
                var graphic=diagram.GetComponent<PaylineGraphic>();graphic.rectTransform.sizeDelta=new Vector2(68,42);graphic.rectTransform.anchoredPosition=new Vector2(0,-7);graphic.raycastTarget=false;graphic.Configure(i+1,false);graphic.color=Mint;
            }
            Text(rules.transform,"RuleLegend","亮色為已啟用線，可點擊在盤面上查看。灰色線需提高線數才會啟用。",new Vector2(0,-290),new Vector2(880,30),16,Muted);
            view.Bind(summary,detail,unit,enabled,pathLabel,path,toggle,previous,next,ruleButton,close,choices,overlay.gameObject);
            controller.BindRefs(null,null,spin,mUp,mDn,bUp,bDn,reels,null);controller.BindPresentation(view);
            overlay.gameObject.SetActive(false);
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/SlotPresentation.unity");
            Debug.Log("[PresentationSceneBuilder] Four-row presentation scene saved.");
        }
        private static Image Image(Transform parent,string name,Vector2 pos,Vector2 size,Color tint)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
            var image=go.GetComponent<Image>();image.rectTransform.anchoredPosition=pos;image.rectTransform.sizeDelta=size;image.color=tint;image.raycastTarget=false;return image;
        }
        private static Text Text(Transform parent,string name,string content,Vector2 pos,Vector2 size,int pixels,Color tint,TextAnchor align=TextAnchor.MiddleCenter)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
            var text=go.GetComponent<Text>();text.font=font;text.text=content;text.fontSize=pixels;text.color=tint;text.alignment=align;text.raycastTarget=false;
            text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;
            text.rectTransform.anchoredPosition=pos;text.rectTransform.sizeDelta=size;return text;
        }
        private static Button Button(Transform parent,string name,string content,Vector2 pos,Vector2 size)
        {
            var image=Image(parent,name,pos,size,new Color(.13f,.23f,.29f));image.raycastTarget=true;
            var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;
            Text(image.transform,"Label",content,Vector2.zero,size,18,Color.white);return button;
        }
        private static void Stretch(RectTransform rt,float inset=0)
        {rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=new Vector2(inset,inset);rt.offsetMax=new Vector2(-inset,-inset);}
    }
}
