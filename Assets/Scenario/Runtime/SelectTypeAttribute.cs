using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Для полей [SerializeReference]: в Inspector появляется выпадающий список с реализациями типа поля.
    /// Название пункта берётся из [DisplayName] класса, иначе из имени класса.
    /// </summary>
    public class SelectTypeAttribute : PropertyAttribute { }
}
