using System;
using UnityEngine;
using XInputDotNetPure;


public class Controller : MonoBehaviour
{
	[SerializeField]
	private float m_fMenuStickReset = 1f;
	private float m_fStickResetTimer = 0f;
	private bool m_bStickReset = true;

	private bool m_bIsInitialize = false;

	private int m_iPlayerNumber = 0;
	public int PlayerNumber
	{
		get { return m_iPlayerNumber; }
	}
	private PlayerIndex m_ePlayerIndex;

	public Player m_cPlayer = null;

	public event Action<bool> OnConnectedStateChange = null;
	public event Action<Vector2> OnLeftStick = null;
	public event Action OnAccept = null;
	public event Action OnCancel = null;

	#region Vibration
	private ControllerVibrationData m_cControllerVibrationData;

	private float m_fVibrationTimer = 0f;

	private bool m_bVibration = false;
	#endregion

	private bool m_bLockInputs = false;
	public bool LockInputs
	{
		get { return m_bLockInputs; }
		set { m_bLockInputs = value; LockGameInputs = value; m_bLockMenuInputs = value; }
	}

    private bool m_bLockGameInputs = false;
    public bool LockGameInputs
    {
        get { return m_bLockGameInputs; }
        set { m_bLockGameInputs = value;  if (m_cPlayer != null && value == true) m_cPlayer.Move(Vector3.zero); }
    }


    public bool m_bLockMenuInputs = false;
	public bool m_bLockInteraction = false;
    private bool m_bLockStateBeforePause = false;
    public bool BlockStateBeforePause
    {
        get { return m_bLockStateBeforePause; }
    }

	private GamePadState m_cState;
	private GamePadState m_cPrevState;

	private bool m_bControllerSet = false;
	private bool m_bIsConnected = false;
	public bool IsConnected
	{
		get { return m_bIsConnected; }
	}


	#region MonoBehavior
	private void Update()
	{
		if (m_bIsInitialize)
		{
			CheckControllerConnected();

			if (m_bControllerSet)
			{
				UpdateGamePadState();

				if (m_cPlayer != null)
					CheckGameInputs();

				CheckMenuInputs();

				if (m_bVibration)
				{
					m_fVibrationTimer -= Time.deltaTime;

					float curveTime = Time.deltaTime / m_cControllerVibrationData.m_fDuration;

					float leftMotor = m_cControllerVibrationData.m_cLeftMotorCurve.Evaluate(curveTime);
					float rightMotor = m_cControllerVibrationData.m_cRightMotorCurve.Evaluate(curveTime);

					GamePad.SetVibration(m_ePlayerIndex, leftMotor, rightMotor);

					if (m_fVibrationTimer <= 0f)
						StopVibration();
				}
			}
		}
	}

	private void OnDestroy()
	{
		StopVibration();

		OnConnectedStateChange = null;
		OnLeftStick = null;
		OnAccept = null;
		OnCancel = null;
	}
	#endregion

	#region Initialization
	public void Init(int controllerIndex)
	{
		if (!m_bIsInitialize)
		{
			m_iPlayerNumber = controllerIndex;
			m_bIsInitialize = true;
		}
	}
	#endregion

	#region Vibration
	public void PlayVibration(ControllerVibrationData vibrationData)
	{
		if (m_bVibration)
			StopVibration();

		m_bVibration = true;

		m_cControllerVibrationData = vibrationData;

		m_fVibrationTimer = vibrationData.m_fDuration;
	}

	public void ResumeVibration()
	{
		if (m_fVibrationTimer > 0f)
			m_bVibration = true;
	}

	public void PauseVibration()
	{
		m_bVibration = false;

		GamePad.SetVibration(m_ePlayerIndex, 0f, 0f);
	}

	public void StopVibration()
	{
		m_bVibration = false;

		m_cControllerVibrationData.m_fDuration = 0f;
		m_fVibrationTimer = 0f;

		m_cControllerVibrationData.m_cLeftMotorCurve = null;
		m_cControllerVibrationData.m_cRightMotorCurve = null;

		GamePad.SetVibration(m_ePlayerIndex, 0f, 0f);
	}
	#endregion

	#region Inputs
	private void CheckGameInputs()
	{
		if (!m_bLockGameInputs && !m_cPlayer.IsFalling)
		{
			m_cPlayer.Move(new Vector3(m_cState.ThumbSticks.Left.X, 0f, m_cState.ThumbSticks.Left.Y));

			if (m_cState.Buttons.A == ButtonState.Pressed && m_cPrevState.Buttons.A == ButtonState.Released && !m_bLockInteraction)
			{
				m_cPlayer.Interact();
			}

			if (m_cState.Buttons.X == ButtonState.Pressed && m_cPrevState.Buttons.X == ButtonState.Released)
			{
				m_cPlayer.Run();
			}

			if (m_cState.Buttons.B == ButtonState.Pressed && m_cPrevState.Buttons.B == ButtonState.Released)
			{
				m_cPlayer.CancelCharacterSelection();
			}
        }
	}

	private void CheckMenuInputs()
	{
		if (!m_bLockMenuInputs)
		{
            float testX = m_cState.ThumbSticks.Left.X;
            float testY = m_cState.ThumbSticks.Left.Y;

            if ((m_cState.ThumbSticks.Left.X != 0f || m_cState.ThumbSticks.Left.Y != 0f) && OnLeftStick != null && m_bStickReset)
			{
				m_fStickResetTimer = 0f;
				m_bStickReset = false;

				OnLeftStick(new Vector2(m_cState.ThumbSticks.Left.X, m_cState.ThumbSticks.Left.Y));
			}

			if (!m_bStickReset)
			{
				m_fStickResetTimer += Time.unscaledDeltaTime;

				m_bStickReset = (m_fStickResetTimer >= m_fMenuStickReset);
			}

			if (m_cState.Buttons.A == ButtonState.Pressed && m_cPrevState.Buttons.A == ButtonState.Released && OnAccept != null)
			{
				OnAccept();
			}

			if (m_cState.Buttons.B == ButtonState.Pressed && m_cPrevState.Buttons.B == ButtonState.Released && OnCancel != null)
			{
				OnCancel();
			}

			if (m_cState.Buttons.Start == ButtonState.Pressed && m_cPrevState.Buttons.Start == ButtonState.Released && m_cPlayer != null)
			{
				if (!GameManager.Instance.IsGamePaused)
					m_bLockStateBeforePause = m_bLockGameInputs;

                m_cPlayer.PauseGame();
			}
		}
	}
	#endregion

	#region ControllerState
	private void CheckControllerConnected()
	{
		if (!m_bControllerSet || !m_cState.IsConnected)
		{
			GamePadState currState = GamePad.GetState((PlayerIndex)m_iPlayerNumber);

			if (currState.IsConnected)
			{
				Debug.Log(string.Format("GamePad found {0}", m_iPlayerNumber + 1));
				m_ePlayerIndex = (PlayerIndex)m_iPlayerNumber;
				m_bControllerSet = true;
				m_bIsConnected = true;

				if (OnConnectedStateChange != null)
					OnConnectedStateChange(true);
			}
			else if (m_bIsConnected)
			{
				m_bIsConnected = false;

				if (OnConnectedStateChange != null)
					OnConnectedStateChange(false);
			}
		}
	}

	private void UpdateGamePadState()
	{
		m_cPrevState = m_cState;
		m_cState = GamePad.GetState(m_ePlayerIndex);
	}
	#endregion
}
