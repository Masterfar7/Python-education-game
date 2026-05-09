# Dialogue Scroll System - Инструкция по настройке

## Описание
Система автоматического скролла для диалогового окна, которая активируется только когда текст превышает определенное количество символов.

## Возможности
- ✅ Автоматическое включение скролла для длинных текстов
- ✅ Автоматическая прокрутка вниз при печати текста
- ✅ Возможность прокрутки вверх для перечитывания
- ✅ Визуальный индикатор (scrollbar)
- ✅ Настраиваемый порог активации

## Настройка в Unity

### Шаг 1: Подготовка UI

1. Откройте сцену с диалоговым окном
2. Найдите объект **DialoguePanel** в иерархии
3. Найдите текстовое поле с диалогом (обычно называется **DialogueText**)

### Шаг 2: Создание ScrollView

1. **Создайте ScrollView:**
   - Правый клик на DialoguePanel → UI → Scroll View
   - Переименуйте в "DialogueScrollView"

2. **Настройте ScrollRect компонент:**
   - Movement Type: Clamped
   - Inertia: ✓ (включено)
   - Deceleration Rate: 0.135
   - Scroll Sensitivity: 1
   - Horizontal: ✗ (выключено)
   - Vertical: ✓ (включено)

3. **Удалите ненужные элементы:**
   - Удалите Horizontal Scrollbar (не нужен)
   - Оставьте Vertical Scrollbar

### Шаг 3: Перенос текста в ScrollView

1. **Переместите DialogueText:**
   - Перетащите ваш TextMeshProUGUI объект с текстом
   - Поместите его внутрь **Content** объекта ScrollView
   - Content → DialogueText

2. **Настройте Content:**
   - Добавьте компонент **Vertical Layout Group**
   - Child Alignment: Upper Center
   - Child Force Expand Height: ✗
   - Добавьте компонент **Content Size Fitter**
   - Vertical Fit: Preferred Size

3. **Настройте DialogueText:**
   - Overflow: Overflow (не Truncate!)
   - Wrapping: Enabled
   - Auto Size: ✗ (выключено)

### Шаг 4: Добавление скрипта

1. **Добавьте DialogueScrollController:**
   - Выберите DialogueScrollView
   - Add Component → Dialogue Scroll Controller

2. **Настройте параметры:**
   - Min Characters For Scroll: 200 (или другое значение)
   - Auto Scroll Down: ✓
   - Auto Scroll Speed: 2
   - Dialogue Text: [перетащите ваш TextMeshProUGUI]
   - Scrollbar: [перетащите Vertical Scrollbar]

### Шаг 5: Подключение к DialogueManager

1. Откройте объект с компонентом **DialogueManager**
2. В секции **Scroll** перетащите DialogueScrollView в поле **Scroll Controller**

### Шаг 6: Настройка Scrollbar (опционально)

Чтобы Scrollbar появлялся только для длинных текстов:

1. Выберите **Scrollbar Vertical**
2. По умолчанию он будет скрыт (скрипт управляет видимостью)

## Настройка параметров

### Min Characters For Scroll
Минимальное количество символов для активации скролла.

**Рекомендуемые значения:**
- 150-200 символов - для маленьких окон
- 200-300 символов - для средних окон
- 300-400 символов - для больших окон

### Auto Scroll Speed
Скорость автоматической прокрутки вниз.

**Рекомендуемые значения:**
- 1-2 - медленная прокрутка
- 2-4 - средняя прокрутка (рекомендуется)
- 4-6 - быстрая прокрутка

## Использование из кода

### Программное управление скроллом

```csharp
DialogueScrollController scroll = GetComponent<DialogueScrollController>();

// Прокрутить вниз
scroll.ScrollToBottom();

// Прокрутить вверх
scroll.ScrollToTop();

// Начать автоскролл
scroll.StartAutoScroll();

// Остановить автоскролл
scroll.StopAutoScroll();

// Сбросить скролл
scroll.ResetScroll();

// Проверить, включен ли скролл
if (scroll.IsScrollEnabled())
{
    Debug.Log("Скролл активен");
}

// Проверить позицию
if (scroll.IsAtBottom())
{
    Debug.Log("Скролл внизу");
}
```

### Изменение порога активации

```csharp
// Установить новый порог
scroll.SetMinCharacters(250);
```

## Визуальная настройка

### Стилизация Scrollbar

1. Выберите **Scrollbar Vertical**
2. Настройте цвета:
   - Normal Color: белый с прозрачностью 0.5
   - Highlighted Color: белый с прозрачностью 0.7
   - Pressed Color: белый с прозрачностью 0.9

3. Настройте Handle (ползунок):
   - Измените спрайт для кастомного вида
   - Настройте размер и отступы

### Анимация появления Scrollbar

Можно добавить Animator для плавного появления:

```csharp
// В DialogueScrollController можно добавить:
private Animator scrollbarAnimator;

private void EnableScroll()
{
    // ... существующий код ...
    
    if (scrollbarAnimator != null)
        scrollbarAnimator.SetTrigger("FadeIn");
}
```

## Примеры использования

### Пример 1: Короткий текст (скролл не появляется)
```
Текст: "Привет! Как дела?"
Символов: 19
Результат: Скролл НЕ активен
```

### Пример 2: Длинный текст (скролл появляется)
```
Текст: "Добро пожаловать в Сад Вечного Цветения. 
Здесь царят циклы. Без них растения не растут, 
кристаллы не горят, а сорняки заполонят всё. 
Чтобы оживить цветок, нужно повторить заклинание 
ровно 5 раз. Ни больше, ни меньше..."
Символов: 250+
Результат: Скролл АКТИВЕН
```

## Troubleshooting

### Проблема: Скролл не работает
**Решение:**
- Проверьте, что ScrollRect компонент включен
- Убедитесь, что Vertical включен в ScrollRect
- Проверьте, что Content Size Fitter добавлен на Content

### Проблема: Текст обрезается
**Решение:**
- Установите Overflow: Overflow в TextMeshProUGUI
- Проверьте Content Size Fitter на Content объекте

### Проблема: Scrollbar не появляется
**Решение:**
- Проверьте, что Scrollbar привязан в инспекторе
- Убедитесь, что текст длиннее порога (Min Characters For Scroll)

### Проблема: Автоскролл слишком быстрый/медленный
**Решение:**
- Измените Auto Scroll Speed (1-6)
- Или отключите Auto Scroll Down

### Проблема: Скролл активируется для коротких текстов
**Решение:**
- Увеличьте Min Characters For Scroll
- Проверьте размер окна и шрифта

## Дополнительные улучшения

### 1. Индикатор "Есть ещё текст"
Добавьте стрелку вниз, которая появляется когда есть непрочитанный текст:

```csharp
public GameObject scrollDownIndicator;

private void Update()
{
    if (scrollDownIndicator != null && isScrollEnabled)
    {
        scrollDownIndicator.SetActive(!IsAtBottom());
    }
}
```

### 2. Звук при скролле
```csharp
public AudioClip scrollSound;
private AudioSource audioSource;

public void OnScroll()
{
    if (audioSource != null && scrollSound != null)
        audioSource.PlayOneShot(scrollSound);
}
```

### 3. Подсветка непрочитанного текста
Можно добавить градиент, который показывает непрочитанную часть.

## Структура файлов

```
Assets/
└── Scripts/
    ├── DialogueScrollController.cs    # Основной скрипт скролла
    ├── DialogueManager.cs             # Обновлен для поддержки скролла
    └── DialogueScrollController_README.md
```

## Горячие клавиши (можно добавить)

```csharp
void Update()
{
    // Прокрутка колесиком мыши
    float scroll = Input.GetAxis("Mouse ScrollWheel");
    if (scroll != 0 && isScrollEnabled)
    {
        scrollRect.verticalNormalizedPosition += scroll * 0.1f;
    }
    
    // Page Up / Page Down
    if (Input.GetKeyDown(KeyCode.PageUp))
        ScrollToTop();
    
    if (Input.GetKeyDown(KeyCode.PageDown))
        ScrollToBottom();
}
```
