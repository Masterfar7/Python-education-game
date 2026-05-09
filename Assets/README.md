# Python Game 1 - Unity Project

## О проекте
Обучающая игра по Python в Unity. Игрок изучает Python через диалоги и интерактивные задания.

## Структура сцен
- **Menu.unity** - Главное меню
- **Chapter1.unity**, **Chapter2.unity**, **Chapter3.unity** - Главы
- **Final.unity** - Экран завершения игры

## Основные системы

### 1. DialogueSystem (DialogueManager.cs)
- Управление диалогами между персонажами (Кай, Аргус, Статуя)
- Показ текста с анимацией печати
- Переход между репликами по кнопке "далее"
- line.startTaskAfterLine - запускает задание после реплики
- line.useLongTextWindow - для длинных текстов

### 2. TaskSystem (TaskSystem.cs)
- Выполнение заданий с вводом Python-кода
- Проверка правильности ответа
- Типы заданий (TaskType):
  - PrintExact, PrintContains, VariableAssignment, ExpressionResult, BooleanValue, BooleanDoorRiddle
- Интеграция с AI для подсказок

### 3. InterpreterEngine (InterpreterEngine.cs)
- Интерпретатор Python в Unity
- Поддерживает: переменные, функции, if/else, while/for, print, списки, f-strings
- Возвращает true/false при Execute()
- lastPrintedValue - последний вывод print

### 4. AI System (TaskAIAdvisor.cs)
- Облачный AI через Cloudflare Workers (Llama 3.1)
- API: https://api.cloudflare.com/client/v4/accounts/{AccountId}/ai/run/{Model}
- Токен: v0evrauS54DVMbYhdhndrc-V9IDmlvkcYE4TjmkY
- Account ID: a0b0bcb493fbd1d65b5be16394b43305
- Функции:
  - EvaluateSolution() - проверка ответа + подсказка
  - Возвращает: correct, hint, error_type (syntax_error/runtime_error/logic_error/none)

### 5. Statistics System (StatisticsManager.cs)
- singleton с DontDestroyOnLoad
- Хранит: попытки, успешные, время игры, строки кода, ошибки по типам
- Путь сохранения: Application.persistentDataPath/statistics.json
- Интеграция:
  - TaskSystem.RecordAttempt() - при pass/fail
  - TaskSystem.RecordCodeLines() - при запуске кода
  - TaskAIAdvisor.RecordError() - при неправильном ответе

### 6. SaveSystem (SaveSystem.cs)
- Сохранение прогресса в JSON файлы
- Слоты: 1-99
- AutoSaveWithAchievements() - автосохранение

## Важные скрипты

| Скрипт | Назначение |
|--------|------------|
| MainMenuController.cs | Управление главным меню |
| DialogueManager.cs | Система диалогов |
| TaskSystem.cs | Выполнение заданий |
| InterpreterEngine.cs | Интерпретатор Python |
| TaskAIAdvisor.cs | AI для подсказок и классификации ошибок |
| StatisticsManager.cs | Сбор статистики |
| StatisticsData.cs | Data class для статистики |
| StatisticsDisplayer.cs | UI для показа статистики в Final.unity |
| SaveSystem.cs | Система сохранений |
| TaskData.cs | Конфигурация задания |
| TaskType.cs | Перечисление типов заданий |

## Известные особенности

### Проблема с longTextWindow
- После выполнения задания longTextWindow может остаться активным
- Решение: в AdvanceDialogueAfterTaskCompleted() добавить HideLongText()

### Сохранение после задания
- Ранее сохранялось после каждого задания, вызывало задержку
- Исправлено: сохранение только в EndDialogue()

### AI подсказки
- Работают через облачный API
- Классифицируют ошибки: syntax_error, runtime_error, logic_error, none

## Интеграция Statistics в Unity

1. **StatisticsManager** добавить на любую сцену (MonoBehaviour,会自动 DontDestroyOnLoad)
2. **StatisticsDisplayer** добавить на Final.unity, привязать TMP_Text
3. Данные сохраняются автоматически при выходе из приложения

## Рабочие процессы

### Добавление нового задания
1. Создать DialogueLine с startTaskAfterLine=true
2. В TaskSystem добавить проверку в CheckTask()
3. При необходимости добавить AI подсказку в TaskAIAdvisor

### Изменение AI промпта
- Редактировать string prompt в TaskAIAdvisor.EvaluateSolution()
- JSON ответ парсится через AiJudgeResult class

## Заметки для AI ( будущих сессий )
- Проект большой, много систем
- AI использует Cloudflare Workers API
- Статистика записывается в JSON локально
- DialogueManager - основной orchestrator
- TaskSystem изолирован от DialogueManager через интерфейс