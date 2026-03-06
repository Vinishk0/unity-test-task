# unity-test-task

Тестовое задание / набор прототипов на Unity с упором на архитектуру, FSM, анимации по Path и UI DataBind.

## Демо

- ▶ Основное демо-видео: [TestUnityWork.mp4](./TestUnityWork.mp4)

## Что внутри


 **TASK3** — сцена и код прототипа (включая FSM и логику лутбокса/слотов).  
  Сцена: [Assets/TASK3/TaskScene.unity](./Assets/TASK3/TaskScene.unity)  
  Скрипты: [Assets/TASK3/Scripts/Lootbox](./Assets/TASK3/Scripts/Lootbox)

## Как запустить

Открой проект через Unity Hub.
Дальше можно стартовать с готовой сцены: `Assets/TASK3/TaskScene.unity` (двойной клик в Project → Open).
Если Unity попросит обновить/перегенерировать Library — это нормально, проект пересоберётся при первом открытии.

## Акценты реализации

В заданиях сделан упор на то, чтобы UI-кнопки меняли состояние через FSM, а уже FSM управляла логикой и перемещениями.
Анимации перемещения предполагаются через Path, а работа с кнопками — через `UIButtonDataBind`.
