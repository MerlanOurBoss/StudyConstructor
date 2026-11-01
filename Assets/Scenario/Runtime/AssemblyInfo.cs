using System.Runtime.CompilerServices;

// Генерация сценария в редакторе и тесты собирают сценарий из кода. Пользователь работает через текст и Inspector.
[assembly: InternalsVisibleTo("Scenario.Editor")]
[assembly: InternalsVisibleTo("Scenario.Tests")]
