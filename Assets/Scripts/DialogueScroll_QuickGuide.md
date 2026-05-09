# Dialogue Scroll - Инструкция (по галочке)

## Как работает
Скролл включается **только когда вы ставите галочку** `Enable Scroll` на нужной реплике в DialogueLine.

## Быстрая настройка

### 1. Настройка UI (один раз)

1. **Создайте ScrollView:**
   - Правый клик на DialoguePanel → UI → Scroll View
   - Переименуйте в "DialogueScrollView"

2. **Переместите текст:**
   - Перетащите ваш **DialogueText** внутрь **Content**
   - Структура: DialogueScrollView → Viewport → Content → DialogueText

3. **Настройте Content:**
   - Add Component → **Vertical Layout Group**
   - Add Component → **Content Size Fitter**
     - Vertical Fit: Preferred Size

4. **Настройте DialogueText:**
   - Overflow: **Overflow** (не Truncate!)
   - Wrapping: Enabled

5. **Добавьте скрипт:**
   - Выберите DialogueScrollView
   - Add Component → **Dialogue Scroll Controller**
   - Dialogue Text: перетащите DialogueText
   - Scrollbar: перетащите Scrollbar Vertical

6. **Подключите к DialogueManager:**
   - Найдите объект с DialogueManager
   - Scroll Controller: перетащите DialogueScrollView

### 2. Использование (для каждой длинной реплики)

1. Откройте NPC с диалогом
2. Найдите нужную реплику в массиве Dialogue Lines
3. Поставьте галочку **Enable Scroll** ✓
4. Готово!

## Пример

```
Реплика 1: "Привет!"
- Enable Scroll: ✗ (короткий текст, скролл не нужен)

Реплика 2: "Добро пожаловать в Сад Вечного Цветения. 
Здесь царят циклы. Без них растения не растут, 
кристаллы не горят, а сорняки заполонят всё. 
Чтобы оживить цветок, нужно повторить заклинание 
ровно 5 раз. Ни больше, ни меньше. Твой инструмент — цикл for..."
- Enable Scroll: ✓ (длинный текст, включаем скролл)

Реплика 3: "Понятно!"
- Enable Scroll: ✗ (короткий текст)
```

## Что происходит

- **Галочка выключена:** Обычное окно диалога, скролла нет
- **Галочка включена:** 
  - Появляется scrollbar справа
  - Текст автоматически прокручивается вниз при печати
  - Игрок может прокрутить вверх для перечитывания

## Настройки в DialogueScrollController

- **Auto Scroll Down:** Автоматически прокручивать вниз (рекомендуется ✓)
- **Auto Scroll Speed:** Скорость прокрутки (2-4 оптимально)

## Troubleshooting

**Проблема:** Галочка стоит, но скролл не появляется
- Проверьте, что Scroll Controller подключен в DialogueManager
- Убедитесь, что Scrollbar Vertical виден (цвет не прозрачный)

**Проблема:** Текст обрезается
- DialogueText → Overflow: должен быть **Overflow**, не Truncate

**Проблема:** Scrollbar всегда виден
- Это нормально, если галочка Enable Scroll включена
- Выключите галочку для коротких реплик

## Преимущества этого подхода

✅ Полный контроль - вы решаете для каких реплик нужен скролл
✅ Просто - одна галочка на реплику
✅ Гибко - можно включить/выключить в любой момент
✅ Наглядно - сразу видно в инспекторе какие реплики со скроллом

Готово! Теперь просто ставьте галочку Enable Scroll на длинных репликах.
