using UnityEngine;

namespace Slot.Game
{
    /// <summary>
    /// Phase 0 placeholder：掛到場景根物件，證明 Slot.Game assembly 能編譯且場景可跑。
    /// Phase 2 將由真正的 SlotGameController 取代。
    /// </summary>
    public class SlotBootstrap : MonoBehaviour
    {
        [Header("Phase 0 smoke test")]
        [SerializeField] private string status = "SlotUnity skeleton OK";

        private void Awake()
        {
            Debug.Log($"[SlotBootstrap] {status} (Slot.Core + Slot.Game assemblies compiled)");
        }
    }
}