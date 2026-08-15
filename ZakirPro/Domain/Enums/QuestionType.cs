namespace ZakirPro.Domain.Enums;

public enum QuestionType
{
    /// <summary>Radio group — 4 choices, exactly one correct.</summary>
    SingleChoice = 0,

    /// <summary>Two choices: True / False, exactly one correct.</summary>
    TrueFalse = 1,

    /// <summary>Free-text textarea — teacher must grade manually.</summary>
    Essay = 2
}
