using TMPro;
using UnityEngine;


public class TutorialTaskActionInstruction : TutorialTaskAction
{
	[SerializeField]
	private LocalizedText m_cInstructionText = null;

	[SerializeField]
	private TextMeshProUGUI m_cInstructionPanelText = null;

	[SerializeField]
	private float m_fTextSpeed = 1f;

	private int m_iInstructionTextLength;
	private float m_fTextLength;

	private bool m_bAnimateText = false;


	#region MonoBehavior
	private void Update()
	{
		if (m_bExecuted && m_bAnimateText)
		{
			m_fTextLength += Time.deltaTime * m_fTextSpeed;

			UpdateInstruction();
		}
	}
	#endregion

	public override void StopExecution()
	{
		base.StopExecution();

		LocalizationManager.Instance.OnLocalizationChanged -= UpdateLanguage;

		m_bAnimateText = false;
	}

	protected override void SetTaskAction()
	{
		TaskAction += () => 
		{
			m_iInstructionTextLength = m_cInstructionText.Text.Length;
			m_fTextLength = 0f;

			m_bAnimateText = true;

			if (!m_bExecuted)
				LocalizationManager.Instance.OnLocalizationChanged += UpdateLanguage;
		};
	}

	private void UpdateInstruction()
	{
		m_bAnimateText = (int)m_fTextLength - 1 < m_iInstructionTextLength;

		if (m_bAnimateText)
		{
			string text = new string(' ', m_iInstructionTextLength);

			string animatedText = m_cInstructionText.Text.Substring(0, (int)m_fTextLength);

			text = text.Remove(0, (int)m_fTextLength);
			text = text.Insert(0, animatedText);

			m_cInstructionPanelText.text = text;
		}
	}

	private void UpdateLanguage()
	{
		if (!m_bAnimateText)
			m_cInstructionPanelText.text = m_cInstructionText.Text;
		else
		{
			m_iInstructionTextLength = m_cInstructionText.Text.Length;

			if ((int)m_fTextLength >= m_iInstructionTextLength)
			{
				m_cInstructionPanelText.text = m_cInstructionText.Text;

				m_bAnimateText = false;
			}
		}
	}
}
