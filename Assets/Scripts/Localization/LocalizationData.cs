using System;


[Serializable]
public class LocalizationData
{
	public LocalizationItem[] m_acLocalizationItems;
}

[Serializable]
public class LocalizationItem
{
	public string m_sKey;
	public string m_sValue;
}
