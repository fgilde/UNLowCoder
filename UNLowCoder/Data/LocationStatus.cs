namespace UNLowCoder.Core.Data;

public enum LocationStatus
{
    Unknown,
    AA, // Approved by competent national government agency
    AC, // Approved by Customs
    AI, // Code adopted by International Organization for Standardization (ISO)
    RL, // Recognized location
    RQ, // Request under consideration
    QQ  // Entry that does not meet UN/LOCODE criteria (e.g., obsolete)    
}