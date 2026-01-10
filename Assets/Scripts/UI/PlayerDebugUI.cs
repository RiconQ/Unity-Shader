using UnityEngine;
using TMPro;
using R3;
using KinematicCharacterController; // KCC 네임스페이스 필요

public class PlayerDebugUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCharacterView m_playerView;
    [SerializeField] private KinematicCharacterMotor m_motor; // 실제 속도/접지 확인용
    [SerializeField] private TextMeshProUGUI m_textUI;

    private void Start()
    {
        if (m_playerView == null || m_textUI == null || m_motor == null)
        {
            Debug.LogError("PlayerDebugUI: 참조가 누락되었습니다. Inspector를 확인해주세요.");
            return;
        }

        // 매 프레임 UI 업데이트
        Observable.EveryUpdate()
            .Subscribe(_ => UpdateDebugText())
            .AddTo(this);
    }

    private void UpdateDebugText()
    {
        var vm = m_playerView.PlayerViewModel;
        if (vm == null) return;

        // 1. 현재 상태 판별 (우선순위 로직)
        string state = "Idle";
        bool isGrounded = m_motor.GroundingStatus.IsStableOnGround;

        if (!isGrounded)
        {
            state = "<color=red>Airborne</color>"; // 공중
        }
        else if (vm.IsSliding.CurrentValue)
        {
            state = "<color=orange>Sliding</color>";
        }
        else if (vm.IsSprinting != null && vm.IsSprinting.CurrentValue) // Dashing이 있다면
        {
            state = "<color=yellow>Dashing</color>";
        }
        else if (vm.IsCrouching.CurrentValue)
        {
            state = vm.IsMoving.CurrentValue ? "Crouch Walk" : "Crouch Idle";
        }
        else if (vm.IsSprinting.CurrentValue && vm.IsMoving.CurrentValue)
        {
            state = "<color=green>Sprinting</color>";
        }
        else if (vm.IsMoving.CurrentValue)
        {
            state = "Walking";
        }

        // 2. 데이터 수집
        float speed = m_motor.Velocity.magnitude;
        float planarSpeed = new Vector3(m_motor.Velocity.x, 0, m_motor.Velocity.z).magnitude; // 수평 속도
        Vector2 input = vm.InputDirection.CurrentValue;

        // 3. 텍스트 조합 (StringBuilder를 쓰면 더 좋지만 간단히 string 보간 사용)
        m_textUI.text =
$@"<b>[Player State]</b>
State: {state}
------------------
<b>[Movement]</b>
Speed (Total):  {speed:F2} m/s
Speed (Planar): {planarSpeed:F2} m/s
Velocity:       {m_motor.Velocity:F1}
------------------
<b>[Input]</b>
WASD: {input:F1}
Grounded: {isGrounded}";
    }
}
