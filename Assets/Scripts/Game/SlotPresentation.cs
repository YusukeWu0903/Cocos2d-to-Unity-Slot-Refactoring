using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Slot.Core;

namespace Slot.Game
{
    public class SlotPresentation : MonoBehaviour
    {
        [SerializeField] private Text summary, detail, unit, enabledLines, pathLabel;
        [SerializeField] private PaylineGraphic path;
        [SerializeField] private Button showPaths, previous, next, rules, close;
        [SerializeField] private Button[] choices;
        [SerializeField] private GameObject rulePanel;
        [SerializeField] private Text cabinetSpinCaption, cabinetUnit, cabinetLines, cabinetMatches, cabinetState;
        [SerializeField] private RetroCabinetGraphic cabinetSpinFace;
        [SerializeField] private Image[] cabinetIndicators;
        private bool Cabinet => cabinetSpinCaption != null;
        public System.Action InspectionRequested;
        private int activeLines=8, selected;
        private bool showPreview, spinning;
        private static readonly Color Cyan=new Color(.35f,.91f,.85f,.8f);
        private static readonly Color Gold=new Color(1f,.81f,.34f,.95f);
        private static readonly string[] SymbolNames={"", "蘋果", "葡萄", "櫻桃", "西瓜", "橘子", "檸檬", "7", "鈴鐺", "GOOD LUCK", "BAR"};

        public void Bind(Text main,Text info,Text unitText,Text lines,Text pathText,PaylineGraphic graphic,
            Button toggle,Button prev,Button forward,Button help,Button dismiss,Button[] lineButtons,GameObject overlay)
        {
            summary=main;detail=info;unit=unitText;enabledLines=lines;pathLabel=pathText;path=graphic;
            showPaths=toggle;previous=prev;next=forward;rules=help;close=dismiss;choices=lineButtons;rulePanel=overlay;
        }
        public void BindCabinet(Text spinCaption,RetroCabinetGraphic spinFace,Text topUnit,Text topLines,Text matches,Text state,Image[] indicators)
        {cabinetSpinCaption=spinCaption;cabinetSpinFace=spinFace;cabinetUnit=topUnit;cabinetLines=topLines;cabinetMatches=matches;cabinetState=state;cabinetIndicators=indicators;}
        private void Details(string normal,string compact) {detail.text=Cabinet?compact:normal;}
        private void CabinetLights(int emphasized=-1,bool win=false)
        {
            if(!Cabinet)return;
            for(int i=0;i<cabinetIndicators.Length;i++)
            {
                var tint=i<activeLines?Color.HSVToRGB((i%8)/8f,.8f,.82f):new Color(.12f,.065f,.17f);
                if(i==emphasized)tint=win?Gold:Cyan;
                cabinetIndicators[i].color=tint;
                cabinetIndicators[i].GetComponentInChildren<Text>().color=i<activeLines?Color.white:new Color(.35f,.29f,.39f);
            }
        }
        private void Start()
        {
            showPaths.onClick.AddListener(()=>{Inspect();showPreview=!showPreview;Preview();});
            previous.onClick.AddListener(()=>{Inspect();selected=(selected+activeLines-1)%activeLines;showPreview=true;Preview();});
            next.onClick.AddListener(()=>{Inspect();selected=(selected+1)%activeLines;showPreview=true;Preview();});
            rules.onClick.AddListener(()=>rulePanel.SetActive(true));
            close.onClick.AddListener(()=>rulePanel.SetActive(false));
            for(int i=0;i<choices.Length;i++)
            {
                int index=i;
                choices[i].onClick.AddListener(()=>{Inspect();selected=index;showPreview=true;rulePanel.SetActive(false);Preview();});
            }
            Preview();
            summary.text="準備就緒";
            Details("調整單位與線數，\n按 SPIN 開始。\n轉動中再按 STOP，\n可提早緩停。","按開始轉動水果盤。\n轉動中再按停輪，可提早緩停。");
            if(Cabinet){cabinetSpinCaption.text="開始\nSTART";cabinetState.text="READY";cabinetMatches.text="00";}
        }
        public void UpdateSettings(int money,int lines)
        {
            activeLines=lines;selected=Mathf.Clamp(selected,0,lines-1);
            unit.text=money.ToString();enabledLines.text=$"{lines} / 40";
            if(Cabinet){cabinetUnit.text=money.ToString("00");cabinetLines.text=lines.ToString("00")+" / 40";}
            for(int i=0;i<choices.Length;i++)
            {
                choices[i].interactable=i<lines&&!spinning;
                choices[i].GetComponent<Image>().color=i<lines?new Color(.16f,.3f,.34f):new Color(.12f,.15f,.2f);
                var graphic=choices[i].GetComponentInChildren<PaylineGraphic>();
                graphic.color=i<lines?Cyan:new Color(.4f,.45f,.5f,.4f);
            }
            Preview();
        }
        private void Inspect()
        {
            InspectionRequested?.Invoke();
            summary.text="支付線預覽";
            Details("每條線從左至右，\n取五軸各一格。\n連續三個以上相符，\n即為中獎。","沿線由左至右取五軸各一格，\n三個以上相符圖案即中獎。");
            if(Cabinet)cabinetState.text="LINES";
        }
        public void SettingsChanged()
        {summary.text="設定已更新";Details("按 SPIN 開始新的一輪。","按開始進行新的一輪。");if(Cabinet)cabinetState.text="READY";}
        private void Preview()
        {
            pathLabel.text=showPreview?$"啟用線 {selected+1:00} / {activeLines}":$"已啟用 {activeLines} 條線";
            showPaths.GetComponentInChildren<Text>().text=showPreview?"隱藏路徑":"查看路徑";
            path.Show(selected+1,5,showPreview&&!spinning,false,Cyan);
            CabinetLights(showPreview&&!spinning?selected:-1);
        }
        public void BeginSpin()
        {
            spinning=true;showPreview=false;rulePanel.SetActive(false);
            Preview();
            summary.text="轉輪中…";Details("按 STOP 可提早緩停。\n五軸停穩後，\n才會判定結果。","按停輪可提早緩停。\n五軸停穩後判定結果。");
            if(Cabinet){cabinetSpinCaption.text="停輪\nSTOP";cabinetSpinFace.SetTone(new Color(.86f,.09f,.32f));cabinetState.text="SPIN";cabinetMatches.text="--";}
            path.Show(1,5,false,false,Cyan);
            showPaths.interactable=previous.interactable=next.interactable=rules.interactable=false;
        }
        public void FinishSpin(IReadOnlyList<WinResult> wins)
        {
            spinning=false;
            if(Cabinet){cabinetSpinCaption.text="開始\nSTART";cabinetSpinFace.SetTone(new Color(.015f,.65f,.35f));cabinetMatches.text=wins.Count.ToString("00");cabinetState.text=wins.Count>0?"WIN":"NO WIN";}
            showPaths.interactable=previous.interactable=next.interactable=rules.interactable=true;
            for(int i=0;i<choices.Length;i++)choices[i].interactable=i<activeLines;
            if(wins.Count==0)
            {showPreview=false;summary.text="本次未中獎";Details("須從最左軸開始，\n沿啟用線連續出現\n至少三個相符圖案。","從最左軸起沿啟用線，\n至少連續三個相符圖案。");Preview();}
            else {summary.text=$"命中 {wins.Count} 條線";ShowWin(wins[0],0,wins.Count);}
        }
        public void ShowWin(WinResult win,int index,int total)
        {
            summary.text=$"命中 {total} 條線";
            Details($"第 {index+1} / {total} 條中獎線\n線 {win.LineId:00} · {SymbolNames[(int)win.Symbol]} {win.Count} 連\n\n框線依序展示\n各條中獎路徑。",$"線 {win.LineId:00} · {SymbolNames[(int)win.Symbol]} {win.Count} 連（{index+1}/{total}）\n中獎路徑依序亮起。");
            pathLabel.text=$"中獎線 {win.LineId:00} · {win.Count} 連";
            path.Show(win.LineId,win.Count,true,true,Gold);
            CabinetLights(win.LineId-1,true);
        }
        public void ShowError()
        {summary.text="暫時無法開始";Details("轉輪或圖案資料不完整。\n請重新載入；若持續發生請回報。","轉輪資料不完整，請重新載入。");path.Show(1,5,false,false,Cyan);if(Cabinet)cabinetState.text="ERROR";}
    }
}
