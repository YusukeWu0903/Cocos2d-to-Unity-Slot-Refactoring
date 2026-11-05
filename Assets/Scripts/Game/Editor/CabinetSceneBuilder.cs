using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Slot.Game;

namespace Slot.EditorTools
{
    public static class CabinetSceneBuilder
    {
        private static Font font;
        private static Color H(string value){ColorUtility.TryParseHtmlString(value,out var color);return color;}
        [MenuItem("Tools/Slot/Build Retro Fruit Cabinet")]
        public static void Build()
        {
            font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/SlotSansTC.ttf");
            if(font==null)throw new System.InvalidOperationException("Missing cabinet font.");
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/SlotPresentation.unity",OpenSceneMode.Single);
            // Save under its own identity before changing the presentation scene.
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/SlotCabinet.unity");
            var roots=scene.GetRootGameObjects();var canvas=roots.Single(o=>o.name=="Canvas").transform;
            var controller=roots.Single(o=>o.name=="SlotRoot").GetComponent<SlotGameController>();
            var view=controller.GetComponent<SlotPresentation>();
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.referenceResolution=new Vector2(1024,800);
            canvas.Find("Background").GetComponent<Image>().color=H("#0c0615");
            foreach(string name in new[]{"Title","Subtitle","Row_0","Row_1","Row_2","Row_3"})Object.DestroyImmediate(canvas.Find(name).gameObject);
            int layer=1;
            var chassis=Panel(canvas,"CabinetGold",Vector2.zero,new Vector2(800,780),RetroCabinetGraphic.Finish.Gold,Color.white);chassis.transform.SetSiblingIndex(layer++);
            var shell=Panel(canvas,"CabinetPurple",Vector2.zero,new Vector2(780,760),RetroCabinetGraphic.Finish.Purple,Color.white);shell.transform.SetSiblingIndex(layer++);
            var marquee=Panel(canvas,"MarqueeGold",new Vector2(0,276),new Vector2(720,94),RetroCabinetGraphic.Finish.Gold,Color.white);marquee.transform.SetSiblingIndex(layer++);
            var marqueeFace=Panel(canvas,"MarqueeFace",new Vector2(0,276),new Vector2(700,76),RetroCabinetGraphic.Finish.Purple,Color.white);marqueeFace.transform.SetSiblingIndex(layer++);
            var bezel=Panel(canvas,"ReelMetal",new Vector2(0,18),new Vector2(600,416),RetroCabinetGraphic.Finish.Chrome,Color.white);bezel.transform.SetSiblingIndex(layer++);
            var deck=Panel(canvas,"ControlDeckGold",new Vector2(0,-323),new Vector2(744,114),RetroCabinetGraphic.Finish.Gold,Color.white);deck.transform.SetSiblingIndex(layer++);
            var deckFace=Panel(canvas,"ControlDeckPurple",new Vector2(0,-323),new Vector2(728,98),RetroCabinetGraphic.Finish.Purple,Color.white);deckFace.transform.SetSiblingIndex(layer++);
            var reelArea=canvas.Find("ReelArea");Rect(reelArea,new Vector2(0,18),new Vector2(560,384));reelArea.GetComponent<Image>().color=H("#eeeede");
            var mask=reelArea.Find("Mask");
            for(int col=0;col<5;col++)Replace(mask.Find("Reel_"+col),RetroCabinetGraphic.Finish.Paper,Color.white);
            for(int edge=0;edge<=5;edge++)
            {
                var separator=Plain(reelArea,"ReelEdge"+edge,new Vector2(-260+edge*104,0),new Vector2(4,372),H("#524c48"));
                separator.transform.SetSiblingIndex(1);
            }
            var logo=Label(canvas,"CabinetTitle","FRUIT SLOT",new Vector2(0,282),new Vector2(470,64),54,H("#fff279"));logo.fontStyle=FontStyle.Bold;
            var outline=logo.gameObject.AddComponent<Outline>();outline.effectColor=H("#0875ad");outline.effectDistance=new Vector2(2,-2);
            var shadow=logo.gameObject.AddComponent<Shadow>();shadow.effectColor=H("#21022f");shadow.effectDistance=new Vector2(0,-5);
            Label(canvas,"CabinetSubtitle","四排水果盤  ·  40 條支付線",new Vector2(0,246),new Vector2(460,24),14,H("#ffe9a1"));
            Fruit(canvas,3,new Vector2(-286,280),new Vector2(58,58));Fruit(canvas,5,new Vector2(286,280),new Vector2(58,58));
            var topUnit=Meter(canvas,"UnitMeter",-282,"押注單位 / UNIT","01");
            var topLines=Meter(canvas,"LinesMeter",-94,"啟用線數 / LINES","08 / 40");
            var matches=Meter(canvas,"MatchesMeter",94,"本輪命中 / MATCH","00");
            var state=Meter(canvas,"StateMeter",282,"回合狀態 / STATUS","READY");state.fontSize=21;
            var indicators=new Image[40];
            for(int i=0;i<40;i++)
            {
                float x=i<20?-320:320;float y=188-(i%20)*18;
                Plain(canvas,"LineRim"+(i+1),new Vector2(x,y),new Vector2(38,18),H("#e9bc63"));
                var face=Plain(canvas,"CabinetLine"+(i+1),new Vector2(x,y),new Vector2(34,15),Color.HSVToRGB((i%8)/8f,.8f,.82f));
                var number=Label(face.transform,"Number",(i+1).ToString("00"),Vector2.zero,new Vector2(34,17),12,Color.white);number.fontStyle=FontStyle.Bold;
                var numberShadow=number.gameObject.AddComponent<Shadow>();numberShadow.effectColor=Color.black;numberShadow.effectDistance=new Vector2(1,-1);
                indicators[i]=face;
            }
            for(int side=-1;side<=1;side+=2)
            {
                var rail=Panel(canvas,"LampRail"+side,new Vector2(side*371,0),new Vector2(20,708),RetroCabinetGraphic.Finish.Gold,Color.white);rail.transform.SetSiblingIndex(layer++);
                for(int bulb=0;bulb<25;bulb++)Panel(canvas,"Lamp"+side+"_"+bulb,new Vector2(side*371,-336+bulb*28),new Vector2(14,14),RetroCabinetGraphic.Finish.Led,Color.HSVToRGB((bulb%6)/6f,.8f,1));
                foreach(float y in new[]{-374f,374f})Panel(canvas,"Screw"+side+"_"+y,new Vector2(side*382,y),new Vector2(10,10),RetroCabinetGraphic.Finish.Screw,Color.white);
            }
            var status=canvas.Find("StatusPanel");Rect(status,new Vector2(0,-229),new Vector2(720,68));Replace(status,RetroCabinetGraphic.Finish.Display,Color.white);
            SetLabel(status.Find("StatusLabel"),"本輪結果 / ROUND",new Vector2(-255,21),new Vector2(190,18),10,H("#e1bb72"),TextAnchor.MiddleLeft);
            SetLabel(status.Find("Summary"),"準備就緒",new Vector2(-255,-5),new Vector2(190,38),24,H("#b9ff87"),TextAnchor.MiddleLeft);
            SetLabel(status.Find("Detail"),"",new Vector2(-20,0),new Vector2(278,58),15,H("#fff1bf"),TextAnchor.MiddleLeft);
            var demoNote=status.Find("DemoNote");demoNote.SetParent(canvas,false);SetLabel(demoNote,"互動展示 · 無實際金幣結算",new Vector2(0,-385),new Vector2(540,18),10,H("#c3a8c9"));
            demoNote.gameObject.SetActive(false);
            SetLabel(canvas.Find("PathLabel"),"已啟用 8 條線",new Vector2(252,-214),new Vector2(184,22),13,H("#8fe8fd"));
            RestyleButton(canvas.Find("TogglePaths"),new Vector2(280,-244),new Vector2(96,27),H("#0a78bf"),"查看路徑",13);
            RestyleButton(canvas.Find("PreviousPath"),new Vector2(190,-244),new Vector2(34,27),H("#6143a7"),"‹",18);
            RestyleButton(canvas.Find("NextPath"),new Vector2(232,-244),new Vector2(34,27),H("#6143a7"),"›",18);
            RestyleButton(canvas.Find("Rules"),new Vector2(70,-324),new Vector2(108,78),H("#0b76b5"),"規則\nHELP",17);
            var bar=canvas.Find("ControlBar");Rect(bar,Vector2.zero,new Vector2(1024,800));bar.GetComponent<Image>().enabled=false;
            SetLabel(bar.Find("UnitLabel"),"押注單位",new Vector2(-284,-285),new Vector2(150,24),14,H("#fff3bd"));
            SetLabel(bar.Find("BranchLabel")??bar.Find("LineLabel"),"啟用線數",new Vector2(-110,-285),new Vector2(160,24),14,H("#fff3bd"));
            SetLabel(bar.Find("UnitValue"),"1",new Vector2(-284,-324),new Vector2(60,42),28,H("#b9ff87"));
            SetLabel(bar.Find("LineValue"),"8 / 40",new Vector2(-110,-324),new Vector2(94,42),24,H("#b9ff87"));
            var unitDisplay=Panel(canvas,"LowerUnitGlass",new Vector2(-284,-324),new Vector2(64,46),RetroCabinetGraphic.Finish.Display,Color.white);unitDisplay.transform.SetSiblingIndex(layer++);
            var lineDisplay=Panel(canvas,"LowerLinesGlass",new Vector2(-110,-324),new Vector2(100,46),RetroCabinetGraphic.Finish.Display,Color.white);lineDisplay.transform.SetSiblingIndex(layer++);
            RestyleButton(bar.Find("MoneyDown"),new Vector2(-336,-324),new Vector2(48,44),H("#bd7118"),"−",22);
            RestyleButton(bar.Find("MoneyUp"),new Vector2(-232,-324),new Vector2(48,44),H("#bd7118"),"+",22);
            RestyleButton(bar.Find("BranchDown"),new Vector2(-180,-324),new Vector2(48,44),H("#0f91bd"),"−",22);
            RestyleButton(bar.Find("BranchUp"),new Vector2(-40,-324),new Vector2(48,44),H("#0f91bd"),"+",22);
            SetLabel(bar.Find("StopHint"),"轉動中可按 STOP 提早緩停",new Vector2(-153,-362),new Vector2(390,20),11,H("#e1c0ec"));
            var spinFace=RestyleButton(bar.Find("SpinButton"),new Vector2(255,-324),new Vector2(200,96),H("#04a659"),"開始\nSTART",25);
            var spinCaption=bar.Find("SpinButton/Label").GetComponent<Text>();spinCaption.fontStyle=FontStyle.Bold;
            view.BindCabinet(spinCaption,spinFace,topUnit,topLines,matches,state,indicators);
            var overlay=canvas.Find("RulesOverlay");var ruleBorder=Panel(overlay,"RetroRulesBorder",Vector2.zero,new Vector2(956,668),RetroCabinetGraphic.Finish.Gold,Color.white);ruleBorder.transform.SetAsFirstSibling();
            overlay.Find("RulesPanel").GetComponent<Image>().color=H("#1b092b");
            overlay.SetAsLastSibling();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            Debug.Log("[CabinetSceneBuilder] Separate retro cabinet scene saved; source presentation scene unchanged.");
        }
        private static Text Meter(Transform parent,string name,float x,string caption,string initial)
        {
            var panel=Panel(parent,name,new Vector2(x,350),new Vector2(178,50),RetroCabinetGraphic.Finish.Display,Color.white);
            Label(panel.transform,"Caption",caption,new Vector2(0,13),new Vector2(162,17),10,H("#f3c67c"));
            var value=Label(panel.transform,"Value",initial,new Vector2(0,-8),new Vector2(164,29),24,H("#b9ff87"));value.fontStyle=FontStyle.Bold;return value;
        }
        private static RetroCabinetGraphic RestyleButton(Transform target,Vector2 pos,Vector2 size,Color tint,string caption,int pixels)
        {
            Rect(target,pos,size);var face=Replace(target,RetroCabinetGraphic.Finish.Button,tint);face.raycastTarget=true;
            target.GetComponent<Button>().targetGraphic=face;
            var text=target.GetComponentInChildren<Text>();text.text=caption;text.fontSize=pixels;text.font=font;Rect(text.transform,Vector2.zero,size-new Vector2(12,8));
            text.color=Color.white;text.alignment=TextAnchor.MiddleCenter;text.verticalOverflow=VerticalWrapMode.Overflow;
            var shadow=text.GetComponent<Shadow>()??text.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.7f);shadow.effectDistance=new Vector2(0,-2);return face;
        }
        private static RetroCabinetGraphic Replace(Transform target,RetroCabinetGraphic.Finish finish,Color tint)
        {
            var image=target.GetComponent<Image>();if(image!=null)Object.DestroyImmediate(image);
            var graphic=target.GetComponent<RetroCabinetGraphic>()??target.gameObject.AddComponent<RetroCabinetGraphic>();graphic.color=Color.white;graphic.raycastTarget=false;graphic.Configure(finish,tint);return graphic;
        }
        private static RetroCabinetGraphic Panel(Transform parent,string name,Vector2 pos,Vector2 size,RetroCabinetGraphic.Finish finish,Color tint)
        {var go=new GameObject(name,typeof(RectTransform),typeof(RetroCabinetGraphic));go.transform.SetParent(parent,false);Rect(go.transform,pos,size);var g=go.GetComponent<RetroCabinetGraphic>();g.raycastTarget=false;g.Configure(finish,tint);return g;}
        private static Image Plain(Transform parent,string name,Vector2 pos,Vector2 size,Color tint)
        {var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);Rect(go.transform,pos,size);var image=go.GetComponent<Image>();image.color=tint;image.raycastTarget=false;return image;}
        private static void Fruit(Transform parent,int id,Vector2 pos,Vector2 size)
        {var image=Plain(parent,"MarqueeFruit"+id,pos,size,Color.white);image.sprite=Resources.Load<Sprite>("Sprites/fruits/icon_fruitbonus_"+id);image.preserveAspect=true;}
        private static Text Label(Transform parent,string name,string text,Vector2 pos,Vector2 size,int pixels,Color tint)
        {var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);SetLabel(go.transform,text,pos,size,pixels,tint);return go.GetComponent<Text>();}
        private static void SetLabel(Transform target,string text,Vector2 pos,Vector2 size,int pixels,Color tint,TextAnchor align=TextAnchor.MiddleCenter)
        {Rect(target,pos,size);var label=target.GetComponent<Text>();label.text=text;label.font=font;label.fontSize=pixels;label.color=tint;label.alignment=align;label.raycastTarget=false;label.verticalOverflow=VerticalWrapMode.Overflow;}
        private static void Rect(Transform target,Vector2 pos,Vector2 size)
        {var rt=(RectTransform)target;rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.anchoredPosition=pos;rt.sizeDelta=size;}
    }
}
