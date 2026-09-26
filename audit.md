## 1. Executive Summary

Текущая DialogueSystem — линейный проигрыватель локализованных реплик с постраничным выводом, печатью текста, форматированием и анимированными текстовыми эффектами.

Фактический Unity-проект расположен в `C:\projects\vano\vano`. Основная реализация находится в `Assets/UI/Dialogue`, но **эта папка не является самостоятельным модулем**: ей необходимы общий текстовый движок, локализация и игровые классы управления вводом.

Ключевые выводы:

- Естественная точка входа — `IDialogueSystem.StartDialogue(Dialogue)`.
- Диалог запускают `NpcDialogue` и `GameplaySceneEntryPoint`.
- Переходами между страницами и репликами управляет `DialogueController`.
- Он же напрямую переключает режимы игрового ввода через `InputModeController`.
- **Уведомления о завершении диалога нет**: отсутствуют callback, событие, возвращаемая операция или публичное состояние сессии.
- UI использует **UI Toolkit: `PanelRenderer`, `Label`, `TextElement`**. Это не Canvas/TMP-реализация.
- Текстовый движок используется также системой `SpeechBubble`, поэтому его принадлежность будущей библиотеке требует отдельного решения.
- UI живёт в корневом контейнере приложения, а контроллер диалога — в контейнере игровой сцены.
- Собственных `asmdef` и автоматических тестов диалоговой системы не найдено.

Аудит выполнен только чтением исходников, сцен, prefab, assets и конфигурации. Unity, сборка и тесты не запускались. Файлы во всех трёх папках не изменялись; extraction не выполнялся.

## 2. Relevant Files

Классификация предварительная:

- **A** — переиспользуемая логика DialogueSystem.
- **B** — переиспользуемый Unity/UI-слой.
- **C** — интеграция и содержимое `vano`.
- **D** — тестовый, временный или legacy-код.
- Смешанная классификация означает, что файл совмещает ответственности.

Все ссылки ниже ведут на фактические абсолютные пути. Сопутствующие `.meta` хранят GUID, важные для сериализованных ссылок; отдельно они не перечислены.

**Диалог и presentation**

| Path | Role | Classification |
|---|---|---|
| [IDialogueSystem.cs](/C:/projects/vano/vano/Assets/UI/Dialogue/IDialogueSystem.cs) | Контракт `StartDialogue` / `EndDialogue` | A |
| [Dialogue.cs](/C:/projects/vano/vano/Assets/UI/Dialogue/Dialogue.cs) | Сериализуемый массив реплик | A |
| [DialogueLine.cs](/C:/projects/vano/vano/Assets/UI/Dialogue/DialogueLine.cs) | Ключи говорящего, текста и presentation | A |
| [DialogueController.cs](/C:/projects/vano/vano/Assets/UI/Dialogue/DialogueController.cs) | Сессия, страницы, реплики, локализация, ввод, закрытие | A + C |
| [DialoguePaginator.cs](/C:/projects/vano/vano/Assets/UI/Dialogue/DialoguePaginator.cs) | Разбиение текста на страницы с сохранением markup | A |
| [DialogueView.cs](/C:/projects/vano/vano/Assets/UI/Dialogue/DialogueView.cs) | UI, измерение текста, typewriter и эффекты | B |
| [DialogueView.uxml](/C:/projects/vano/vano/Assets/UI/Dialogue/DialogueView.uxml) | Дерево элементов диалога | B |
| [DialogueView.uss](/C:/projects/vano/vano/Assets/UI/Dialogue/DialogueView.uss) | Размеры, расположение и оформление | B; конкретный дизайн требует решения |
| [DialoguePresentation.cs](/C:/projects/vano/vano/Assets/UI/Dialogue/Presentation/DialoguePresentation.cs) | Runtime-настройки отображения | A |
| [DialogueTypingMode.cs](/C:/projects/vano/vano/Assets/UI/Dialogue/Presentation/DialogueTypingMode.cs) | `Typewriter` / `Instant` | A |
| [DialoguePresentationIds.cs](/C:/projects/vano/vano/Assets/UI/Dialogue/Presentation/DialoguePresentationIds.cs) | Идентификатор `standard` | A |
| [DialoguePresentationDefinition.cs](/C:/projects/vano/vano/Assets/UI/Dialogue/Presentation/DialoguePresentationDefinition.cs) | Сериализуемое описание presentation | B |
| [DialoguePresentationSettings.cs](/C:/projects/vano/vano/Assets/UI/Dialogue/Presentation/DialoguePresentationSettings.cs) | ScriptableObject со списком presentations | B |
| [DialoguePresentationResolver.cs](/C:/projects/vano/vano/Assets/UI/Dialogue/Presentation/DialoguePresentationResolver.cs) | Поиск presentation и fallback | A/B: принимает Unity asset |
| [DialoguePresentations.asset](/C:/projects/vano/vano/Assets/Game/Config/UI/DialoguePresentations.asset) | Настроенные `standard` и `system` | C; возможный пример |

**Общий текстовый движок — прямая зависимость диалога**

| Path | Role | Classification |
|---|---|---|
| [TextToken.cs](/C:/projects/vano/vano/Assets/UI/Text/TextToken.cs) | Общая модель текста и тегов | A |
| [TextTokenParser.cs](/C:/projects/vano/vano/Assets/UI/Text/TextTokenParser.cs) | Разбор квадратных тегов DSL | A |
| [TextWhitespaceNormalizer.cs](/C:/projects/vano/vano/Assets/UI/Text/TextWhitespaceNormalizer.cs) | Нормализация пробелов и переносов | A |
| [TextMarkupValidator.cs](/C:/projects/vano/vano/Assets/UI/Text/Markup/TextMarkupValidator.cs) | Проверка тегов, вложенности и аргументов | A |
| [TextMarkupDiagnostics.cs](/C:/projects/vano/vano/Assets/UI/Text/Markup/TextMarkupDiagnostics.cs) | Предупреждения Unity в Editor/Development Build | B |
| [TextTypewriter.cs](/C:/projects/vano/vano/Assets/UI/Text/Typewriter/TextTypewriter.cs) | Корутинное воспроизведение текста и команд | B |
| [TextTypewriterState.cs](/C:/projects/vano/vano/Assets/UI/Text/Typewriter/TextTypewriterState.cs) | Состояния печати | A |
| [TextTypewriterToken.cs](/C:/projects/vano/vano/Assets/UI/Text/Typewriter/TextTypewriterToken.cs) | Базовый token и текстовый token | A |
| [TextTypewriterTokenParser.cs](/C:/projects/vano/vano/Assets/UI/Text/Typewriter/TextTypewriterTokenParser.cs) | Преобразование DSL в команды печати | A |
| [TextTypewriterCadence.cs](/C:/projects/vano/vano/Assets/UI/Text/Typewriter/TextTypewriterCadence.cs) | Задержки по пунктуации; использует settings | A/B |
| [TextTypewriterTimingSettings.cs](/C:/projects/vano/vano/Assets/UI/Text/Typewriter/TextTypewriterTimingSettings.cs) | ScriptableObject с параметрами задержек | B |
| [TextTypewriterCommand.cs](/C:/projects/vano/vano/Assets/UI/Text/Typewriter/Commands/TextTypewriterCommand.cs) | Базовая команда | A |
| [EventTextCommand.cs](/C:/projects/vano/vano/Assets/UI/Text/Typewriter/Commands/EventTextCommand.cs) | Идентификатор текстового события | A |
| [PauseTextCommand.cs](/C:/projects/vano/vano/Assets/UI/Text/Typewriter/Commands/PauseTextCommand.cs) | Пауза на заданное время | A |
| [WaitTextCommand.cs](/C:/projects/vano/vano/Assets/UI/Text/Typewriter/Commands/WaitTextCommand.cs) | Ожидание подтверждения | A |
| [PushSpeedTextCommand.cs](/C:/projects/vano/vano/Assets/UI/Text/Typewriter/Commands/PushSpeedTextCommand.cs) / [PopSpeedTextCommand.cs](/C:/projects/vano/vano/Assets/UI/Text/Typewriter/Commands/PopSpeedTextCommand.cs) | Вход/выход из области скорости | A |
| [PushInstantTextCommand.cs](/C:/projects/vano/vano/Assets/UI/Text/Typewriter/Commands/PushInstantTextCommand.cs) / [PopInstantTextCommand.cs](/C:/projects/vano/vano/Assets/UI/Text/Typewriter/Commands/PopInstantTextCommand.cs) | Вход/выход из области мгновенного вывода | A |
| [TextRichToken.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/TextRichToken.cs) | Текст с форматами, стилями и эффектами | A |
| [TextRichTokenParser.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/TextRichTokenParser.cs) | Разбор визуальной части DSL | A/B: связан с registry обработчиков |
| [TextRichRenderer.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/TextRichRenderer.cs) | Rich text и обработка геометрии символов UI Toolkit | B |
| [TextRichFormat.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Format/TextRichFormat.cs) / [TextRichStyle.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Format/TextRichStyle.cs) | Модели форматирования и именованных стилей | A |
| [UnityRichTextFormatConverter.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Format/UnityRichTextFormatConverter.cs) | Преобразование форматов в Unity rich-text tags | B по назначению |
| [TextRichEffect.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Effect/TextRichEffect.cs) | Базовая модель эффекта | A |
| [ITextRichEffectHandler.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Effect/ITextRichEffectHandler.cs) / [TextRichEffectHandler.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Effect/TextRichEffectHandler.cs) | Контракт и база обработчиков glyph | B |
| [TextRichEffectRegistry.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Effect/TextRichEffectRegistry.cs) | Реестр обработчиков, общий `Default` | B |
| [TextRichEffectWave.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Effect/TextRichEffectWave.cs) / [TextRichEffectWaveHandler.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Effect/TextRichEffectWaveHandler.cs) | Модель и реализация волны | A / B |
| [TextRichEffectShake.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Effect/TextRichEffectShake.cs) / [TextRichEffectShakeHandler.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Effect/TextRichEffectShakeHandler.cs) | Модель и реализация дрожания | A / B |
| [TextRichEffectRainbow.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Effect/TextRichEffectRainbow.cs) / [TextRichEffectRainbowHandler.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Effect/TextRichEffectRainbowHandler.cs) | Модель и реализация изменения цветов | A / B |
| [TextRichEffectBounce.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Effect/TextRichEffectBounce.cs) / [TextRichEffectBounceHandler.cs](/C:/projects/vano/vano/Assets/UI/Text/Rich/Effect/TextRichEffectBounceHandler.cs) | Модель и реализация подпрыгивания | A / B |
| [DefaultTypewriterTiming.asset](/C:/projects/vano/vano/Assets/Text/Settings/DefaultTypewriterTiming.asset) | Назначенные view параметры печати | B; конкретная настройка |

**Игровая интеграция и локализация**

| Path | Role | Classification |
|---|---|---|
| [NpcDialogue.cs](/C:/projects/vano/vano/Assets/Core/NpcDialogue.cs) | Хранит диалог NPC и запускает его при взаимодействии | C |
| [IInteractable.cs](/C:/projects/vano/vano/Assets/Core/IInteractable.cs) | Контракт игрового взаимодействия | C относительно DialogueSystem |
| [PlayerInteractor.cs](/C:/projects/vano/vano/Assets/Core/PlayerInteractor.cs) | Выбирает цель по Collider2D, расстоянию и направлению | C |
| [PlayerController.cs](/C:/projects/vano/vano/Assets/Core/PlayerController.cs) | Связывает ввод, движение и взаимодействие | C |
| [PlayerInputController.cs](/C:/projects/vano/vano/Assets/Core/PlayerInputController.cs) | Игровой ввод и остановка движения при отключении | C |
| [CharacterController.cs](/C:/projects/vano/vano/Assets/Core/CharacterController.cs) | Направление персонажа, движение и остановка | C |
| [Mover.cs](/C:/projects/vano/vano/Assets/Core/Mover.cs), [CharacterAnimator.cs](/C:/projects/vano/vano/Assets/Core/CharacterAnimator.cs), [Direction.cs](/C:/projects/vano/vano/Assets/Core/Direction.cs), [CharacterAnimationState.cs](/C:/projects/vano/vano/Assets/Core/CharacterAnimationState.cs) | Транзитивная игровая цепочка движения/анимации | C |
| [DialogueInputController.cs](/C:/projects/vano/vano/Assets/Core/DialogueInputController.cs) | Преобразует Input Actions в `Submit` / `Cancel` | C в текущем виде; кандидат на B после обсуждения |
| [InputModeController.cs](/C:/projects/vano/vano/Assets/Input/InputModeController.cs) | Переключает ввод игрока и диалога | C |
| [GameInputActions.cs](/C:/projects/vano/vano/Assets/Input/GameInputActions.cs) | Создаёт и освобождает `PlayerControls` | C |
| [PlayerControls.cs](/C:/projects/vano/vano/Assets/Settings/PlayerControls.cs) / [PlayerInput.inputactions](/C:/projects/vano/vano/Assets/Settings/PlayerInput.inputactions) | Сгенерированный класс и карты `Game` / `Dialogue` | C |
| [InputSystem_Actions.inputactions](/C:/projects/vano/vano/Assets/Settings/InputSystem_Actions.inputactions) | Общий actions asset из настроек проекта; отдельный от указанной цепочки | C |
| [ILocalizationService.cs](/C:/projects/vano/vano/Assets/Localization/ILocalizationService.cs) | Проектный контракт получения строк и смены языка | A/C: принадлежность не определена |
| [UnityLocalizationService.cs](/C:/projects/vano/vano/Assets/Localization/UnityLocalizationService.cs) | Адаптер Unity Localization | B/C |
| [LocalizationTables.cs](/C:/projects/vano/vano/Assets/Localization/LocalizationTables.cs) | Жёстко заданное имя таблицы `Dialogue` | C в текущей связке |
| [Localization Settings.asset](</C:/projects/vano/vano/Assets/Localization/Localization Settings.asset>) | Инициализация и выбор локали | C |
| [English (en).asset](</C:/projects/vano/vano/Assets/Localization/Locales/English (en).asset>) / [Russian (ru).asset](</C:/projects/vano/vano/Assets/Localization/Locales/Russian (ru).asset>) | Доступные локали | C |
| [Dialogue.asset](/C:/projects/vano/vano/Assets/Localization/Tables/Dialogue/Dialogue.asset) | Editor-коллекция таблиц | C |
| [Dialogue Shared Data.asset](</C:/projects/vano/vano/Assets/Localization/Tables/Dialogue/Dialogue Shared Data.asset>) | Общие ключи и ID строк | C |
| [Dialogue_en.asset](/C:/projects/vano/vano/Assets/Localization/Tables/Dialogue/Dialogue_en.asset) / [Dialogue_ru.asset](/C:/projects/vano/vano/Assets/Localization/Tables/Dialogue/Dialogue_ru.asset) | Тексты и примеры markup | C + D |

**Сборка приложения, UI assets и сцены**

| Path | Role | Classification |
|---|---|---|
| [AppLifetimeScope.cs](/C:/projects/vano/vano/Assets/Infrastructure/DI/AppLifetimeScope.cs) | Регистрирует общий UI и localization service | C |
| [GameplaySceneLifetimeScope.cs](/C:/projects/vano/vano/Assets/Infrastructure/DI/GameplaySceneLifetimeScope.cs) | Собирает контроллер диалога и игровой ввод | C |
| [VContainerSettings.asset](/C:/projects/vano/vano/Assets/Infrastructure/DI/VContainerSettings.asset) | Назначает корневой prefab | C |
| [AppRoot.prefab](/C:/projects/vano/vano/Assets/Infrastructure/DI/AppRoot.prefab) | Общий root, bootstrap, PanelRenderer, DialogueView, WorldUIRoot | C; содержит B |
| [AppBootstrap.cs](/C:/projects/vano/vano/Assets/Application/Bootstrap/AppBootstrap.cs) | Ждёт локализацию и UI, загружает Gameplay | C |
| [GameplaySceneEntryPoint.cs](/C:/projects/vano/vano/Assets/Infrastructure/Bootstrap/GameplaySceneEntryPoint.cs) | Запускает intro либо включает игровой ввод | C |
| [GameplaySceneConfig.cs](/C:/projects/vano/vano/Assets/Infrastructure/Bootstrap/GameplaySceneConfig.cs) | Сериализованный intro dialogue | C |
| [SceneReference.cs](/C:/projects/vano/vano/Assets/Application/Scenes/SceneReference.cs) / [SceneReferenceDrawer.cs](/C:/projects/vano/vano/Assets/Editor/SceneReferenceDrawer.cs) | Ссылка на сцену и её инспектор | C |
| [PlayModeStartSceneMenu.cs](/C:/projects/vano/vano/Assets/Editor/PlayModeStartSceneMenu.cs) | Выбор стартовой сцены в Editor | C |
| [PanelSettings.asset](</C:/projects/vano/vano/Assets/UI Toolkit/PanelSettings.asset>) | Настройки панели и ссылка на text settings | B/C |
| [DialogueTextSettings.asset](</C:/projects/vano/vano/Assets/UI Toolkit/DialogueTextSettings.asset>) | `PanelTextSettings`, подключение style sheet | B |
| [DialogueTextStyles.asset](</C:/projects/vano/vano/Assets/UI Toolkit/DialogueTextStyles.asset>) | TextCore-стили `Normal` и `warning` | B; оформление — решение |
| [UnityDefaultRuntimeTheme.tss](</C:/projects/vano/vano/Assets/UI Toolkit/UnityThemes/UnityDefaultRuntimeTheme.tss>) | Импорт стандартной Unity-темы | B |
| [Bootstrap.unity](/C:/projects/vano/vano/Assets/Scenes/Bootstrap.unity) | Начальная сцена приложения | C |
| [Gameplay.unity](/C:/projects/vano/vano/Assets/Scenes/Gameplay.unity) | Игрок, NPC, DI scope и ручные примеры диалога | C + D |
| [0.unity](/C:/projects/vano/vano/Assets/_Recovery/0.unity) | Recovery-снимок старой конфигурации диалога | D |
| [manifest.json](/C:/projects/vano/vano/Packages/manifest.json) / [packages-lock.json](/C:/projects/vano/vano/Packages/packages-lock.json) | Объявленные и разрешённые версии пакетов | C |
| [ProjectVersion.txt](/C:/projects/vano/vano/ProjectSettings/ProjectVersion.txt) | Unity `6000.5.6f1` | C |
| [ProjectSettings.asset](/C:/projects/vano/vano/ProjectSettings/ProjectSettings.asset) | Preloaded VContainer settings и режим ввода | C |
| [EditorBuildSettings.asset](/C:/projects/vano/vano/ProjectSettings/EditorBuildSettings.asset) | Bootstrap/Gameplay и ссылки на конфигурацию пакетов | C |

Локализация также опирается на [AddressableAssetSettings.asset](/C:/projects/vano/vano/Assets/AddressableAssetsData/AddressableAssetSettings.asset), [DefaultObject.asset](/C:/projects/vano/vano/Assets/AddressableAssetsData/DefaultObject.asset) и следующие группы. Их стандартные schemas/builders относятся к инфраструктуре проекта, а не к коду диалога.

| Path | Role | Classification |
|---|---|---|
| [Localization-Assets-Shared.asset](/C:/projects/vano/vano/Assets/AddressableAssetsData/AssetGroups/Localization-Assets-Shared.asset) | Shared Table Data | C |
| [Localization-Locales.asset](/C:/projects/vano/vano/Assets/AddressableAssetsData/AssetGroups/Localization-Locales.asset) | Локали en/ru | C |
| [Localization-String-Tables-English (en).asset](</C:/projects/vano/vano/Assets/AddressableAssetsData/AssetGroups/Localization-String-Tables-English (en).asset>) | `Dialogue_en`, метка `Preload` | C |
| [Localization-String-Tables-Russian (ru).asset](</C:/projects/vano/vano/Assets/AddressableAssetsData/AssetGroups/Localization-String-Tables-Russian (ru).asset>) | `Dialogue_ru`, метка `Preload` | C |

**Смежная система SpeechBubble и legacy**

| Path | Role | Classification |
|---|---|---|
| [SpeechBubbleView.cs](/C:/projects/vano/vano/Assets/UI/SpeechBubble/SpeechBubbleView.cs) | Второй потребитель общего rich-text движка | B; вне минимального диалога |
| [SpeechBubbleView.uxml](/C:/projects/vano/vano/Assets/UI/SpeechBubble/SpeechBubbleView.uxml) / [SpeechBubbleView.uss](/C:/projects/vano/vano/Assets/UI/SpeechBubble/SpeechBubbleView.uss) | UI облачка | B |
| [SpeechBubbleModel.cs](/C:/projects/vano/vano/Assets/UI/SpeechBubble/SpeechBubbleModel.cs) | Текст, время и видимость облачка | A смежной системы |
| [SpeechBubbleController.cs](/C:/projects/vano/vano/Assets/UI/SpeechBubble/SpeechBubbleController.cs) | Камера, world-to-panel, таймер и DI | B + C |
| [SpeechBubbleAnchor.cs](/C:/projects/vano/vano/Assets/UI/SpeechBubble/SpeechBubbleAnchor.cs) | Мировая позиция облачка | B |
| [WorldUIRoot.cs](/C:/projects/vano/vano/Assets/UI/World/WorldUIRoot.cs) | Общий UI-контейнер мировых элементов | B; сборка приложения — C |
| [SpeechBubbleTest.cs](/C:/projects/vano/vano/Assets/UI/SpeechBubble/SpeechBubbleTest.cs) | Автоматический показ тестовой строки после задержки | D |
| [typewriter.txt](/C:/projects/vano/scripts/typewriter.txt) | Старая версия typewriter вне Unity Assets | D |
| [vano_game_plan.md](/C:/projects/vano/docs/vano_game_plan.md) | Планы cleanup, локализации и будущей Event System | C; не доказательство реализации |

## 3. Current Architecture

`DialogueController` — обычный C#-объект, не `MonoBehaviour`. Он получает через конструктор конкретный `DialogueView`, контроллер диалогового ввода, переключатель режимов, localization service и resolver presentation.

В одном классе объединены:

- состояние текущего диалога;
- индексы реплики и страницы;
- обработка `Submit` / `Cancel`;
- разрешение ключей локализации;
- выбор presentation;
- запуск пагинации и отображения;
- переключение игрового ввода.

`DialogueView` отвечает за Unity/UI Toolkit и создаёт два `TextRichRenderer`: видимый и измерительный. Также он создаёт `TextTypewriter` и передаёт ему себя как владельца корутин.

`DialoguePaginator` не знает Unity напрямую: размер текста проверяется через переданный `Func<string, bool>`. Однако фактическая пагинация зависит от текущей геометрии UI через `DialogueView.CanFitText`.

Presentation разделена на сериализуемую конфигурацию и runtime-объект. Resolver собирает словарь один раз. Неизвестный/пустой ID возвращает `standard`; если корректного `standard` нет, используется встроенный вариант с typewriter, видимым speaker и без CSS-класса. Записи без `styleClass` игнорируются.

Общий текстовый движок — самостоятельная значительная подсистема. Его rich-text часть используется и диалогом, и `SpeechBubbleView`. Обратной зависимости текстового движка от `DialogueController` не обнаружено.

Прямой зависимости диалога от quest system, save/load, камеры или игровой Event System нет. Текстовые `[event=...]` сейчас заканчиваются записью в лог. Упоминания будущей Event System в плане проекта не являются существующей интеграцией.

## 4. Runtime Call Flow

**Инициализация**

1. `ProjectSettings.asset` содержит `VContainerSettings` в preloaded assets.
2. В настройках указан `AppRoot.prefab`.
3. Код установленного VContainer создаёт root scope и вызывает `DontDestroyOnLoad`.
4. `AppLifetimeScope` регистрирует `DialogueView`, `WorldUIRoot` и singleton `ILocalizationService`.
5. `DialogueView.Awake()` требует назначенный `TextTypewriterTimingSettings`.
6. После UI reload view находит элементы по именам, создаёт текстовые системы и скрывает окно.
7. `IsBootstrapped` становится `true`, когда viewport получает конечные положительные размеры.
8. `AppBootstrap` ждёт успешной инициализации Localization и готовности view, затем загружает `Gameplay` в режиме `Single`.
9. Gameplay scope создаёт свои scoped-сервисы и запускает `GameplaySceneEntryPoint`.

У ожидания геометрии UI нет таймаута.

**Запуск через NPC**

```text
PlayerControls.Game.Interact — Enter
  → PlayerInputController.Interact
  → PlayerController.OnInteract()
  → PlayerInteractor.Interact(FacingDirection)
  → выбор ближайшего подходящего IInteractable
  → NpcDialogue.Interact()
  → IDialogueSystem.StartDialogue(serialized Dialogue)
```

В текущей сцене NPC находится под `Entities`. Этот объект включён в `autoInjectGameObjects`; установленный VContainer рекурсивно внедряет зависимости в дочерние объекты. Таким образом, для данного NPC путь инъекции подтверждён сериализованной сценой и кодом контейнера.

**Запуск при входе в сцену**

`GameplaySceneEntryPoint.Start()` вызывает `StartDialogue(config.IntroDialogue)`, если объект не `null`.

В сохранённой `Gameplay.unity` intro содержит `lines: []`. При обработке такого диалога контроллер сразу завершает его и возвращает игровой ввод.

**Отображение**

`StartDialogue()`:

1. Игнорирует `null`.
2. Сохраняет ссылку на переданный объект без копирования.
3. Сбрасывает индекс реплики и устанавливает активность.
4. Вызывает `inputMode.SetDialogue()`.
5. Показывает view.
6. Разрешает presentation, speaker и text текущей реплики.
7. Проверяет markup диагностикой.
8. Делит текст на страницы по размеру view.
9. Показывает первую страницу через typewriter либо мгновенно.

Повторный запуск во время активного диалога заменяет текущий диалог. Очереди и уведомления о прерывании нет.

**Продвижение**

`DialogueInputController` преобразует Enter в `Submit`. Дальнейший приоритет строго такой:

| Состояние | Действие контроллера |
|---|---|
| Typewriter ожидает `[wait]` | `ContinueText()` |
| Текст ещё печатается | `CompleteText()` |
| Есть следующая страница | Показ следующей страницы |
| Страницы закончились | `Next()` — следующая реплика |
| Реплики закончились | `EndDialogue()` |

`CompleteText()` раскрывает текст до ближайшего `[wait]` или конца страницы. Паузы пропускаются, текстовые события выполняются.

Публичный `DialogueController.Next()` не входит в `IDialogueSystem`: он сразу переключает реплику, обходя обычную логику страниц и ожиданий.

**Завершение и ввод**

Escape вызывает `Cancel`, затем `EndDialogue()`. Тот же метод вызывается после последней реплики либо напрямую через интерфейс.

`EndDialogue()`:

- снимает внутренний флаг активности;
- скрывает view и останавливает typewriter;
- безусловно включает игровой режим.

Вызывающий код **не получает сообщение о завершении** и не может различить нормальное окончание, Escape и внешнее закрытие.

Блокировкой управляет `InputModeController`: отключает `Game` и включает `Dialogue`. Отключение `PlayerInputController` очищает нажатые направления и отправляет `StopRequested`, после чего `PlayerController` останавливает персонажа. Это блокировка игрового ввода, а не остановка всего мира или изменение `Time.timeScale`.

**Освобождение и смена сцены**

`DialogueController.Dispose()` только отписывается от input и text events. Он не вызывает `EndDialogue()`.

`DialogueView.OnDisable()` отписывается от UI reload/geometry, останавливает typewriter, освобождает renderer и сбрасывает готовность.

Поскольку view сохраняется в root scope, выгрузка gameplay scope сама по себе не гарантирует скрытие активного диалога. Это риск, видимый из кода; сценарий смены сцены во время диалога не запускался.

## 5. Dependency Graph

`[!]` обозначает связь, препятствующую независимому извлечению текущей реализации без определения границ.

```text
Игровое взаимодействие
PlayerInputController
  → PlayerController
    → CharacterController → Mover / CharacterAnimator
    → PlayerInteractor → IInteractable
      → NpcDialogue
        → IDialogueSystem

GameplaySceneEntryPoint
  → GameplaySceneConfig → Dialogue
  → IDialogueSystem

IDialogueSystem
  ← DialogueController
      → Dialogue → DialogueLine
      → DialoguePresentationResolver
          → DialoguePresentationSettings
          → DialoguePresentationDefinition
          → DialoguePresentation / DialogueTypingMode
      → DialoguePaginator
          → TextWhitespaceNormalizer
          → TextTokenParser → TextToken
          → DialogueView.CanFitText (callback)
      → DialogueView
      → TextMarkupDiagnostics → TextMarkupValidator
      → [!] ILocalizationService / LocalizationTables
      → [!] DialogueInputController
          → GameInputActions → PlayerControls → Input System
      → [!] InputModeController
          → PlayerInputController
          → DialogueInputController

DialogueView
  → Unity UI Toolkit: PanelRenderer / Label / TextElement
  → UXML / USS / PanelSettings / PanelTextSettings / TextStyleSheet
  → TextRichTokenParser → TextRichToken / Format / Style / Effect
  → TextRichRenderer
      → UnityRichTextFormatConverter
      → TextRichEffectRegistry.Default → effect handlers
  → TextTypewriter
      → TextTypewriterTokenParser / commands / state
      → TextTypewriterCadence → TextTypewriterTimingSettings
      → MonoBehaviour coroutines / Unity Time

SpeechBubbleView
  → общий TextTokenParser / Normalizer / Diagnostics
  → общий TextRichTokenParser / TextRichRenderer / EffectRegistry

[!] Сборка и время жизни
ProjectSettings.preloadedAssets
  → VContainerSettings → AppRoot.prefab (persistent)
      → AppLifetimeScope → DialogueView / localization
      → AppBootstrap → internal DialogueView.IsBootstrapped
      → PanelRenderer / WorldUIRoot

GameplaySceneLifetimeScope
  → scoped DialogueController / input / resolver
  → PlayerInputController из сцены
  → autoInject Entities → NpcDialogue

UnityLocalizationService
  → LocalizationSettings
  → StringDatabase → Dialogue tables
  → Addressables / locales / preload configuration
```

Прямого цикла зависимостей между классами не найдено. Однако граница предполагаемой библиотеки сейчас пересекается в обе стороны: игра вызывает `IDialogueSystem`, а реализация интерфейса вызывает игровые классы управления состоянием ввода.

## 6. External / Game-Specific Dependencies

Версии взяты из локальной конфигурации, а не из предположений о текущих версиях пакетов.

| Dependency | Used by | Why | Extraction impact |
|---|---|---|---|
| Unity `6000.5.6f1`, UI Toolkit | View, renderer, handlers | Панель, labels, измерение, обработка glyph | Обязательны для текущего UI; совместимость с более ранними Unity не проверена |
| `UnityEngine` | View, typewriter, settings, diagnostics | MonoBehaviour, корутины, Time, ScriptableObject, Debug | Обязателен текущему Unity-слою |
| `com.unity.modules.uielements` `1.0.0` | UI-слой | Модуль UI Toolkit | Прямая зависимость |
| Input System `1.20.0` | `DialogueInputController`, `PlayerControls`, игровой ввод | Submit/Cancel и action maps | Нужен текущему вводу; не используется моделями, paginator и renderer |
| VContainer `1.19.0` | Scope, entry point, NPC, PlayerInputController | Сборка зависимостей и lifetime | Интеграционная зависимость; `DialogueController` и `DialogueView` VContainer не импортируют |
| Unity Localization `1.5.12` | `UnityLocalizationService`, `AppBootstrap` | Получение строк, локали, инициализация | Нужен текущему адаптеру; контроллер зависит от проектного интерфейса |
| Addressables `2.9.1` в lock-файле | Инфраструктура Localization | Доставка и preload таблиц/локалей | Транзитивная runtime-зависимость текущего способа локализации |
| `DialogueInputController` | `DialogueController` | События ввода | Прямая зависимость от класса за пределами диалоговой папки |
| `InputModeController` | `DialogueController` | Переключение Gameplay/Dialogue | Прямо втягивает `PlayerInputController` |
| `GameInputActions` / `PlayerControls` | Диалоговый ввод | Конкретные игровые action maps | Без них текущий input adapter не компилируется |
| `ILocalizationService` | `DialogueController` | Получение текста и speaker | Контракт находится вне модуля; принадлежность требует решения |
| `LocalizationTables.Dialogue` | `DialogueController` | Выбор единственной таблицы | Жёсткая привязка к соглашению `vano` |
| `Assets/UI/Text` | Controller, paginator, view | Markup, печать, эффекты | Необходимая внутренняя зависимость; совместно используется SpeechBubble |
| Bootstrap, prefab, scene scope | Вся рабочая сборка | Создание, настройки, готовность UI | Runtime-зависимость, которую нельзя заменить копированием C# |
| TextCore style sheet и тема | Rich-text вывод | `[style=warning]`, базовое оформление | Нужны для воспроизведения текущего вида |
| TMP / `com.unity.ugui` `2.5.0` | Прямого использования в диалоге нет | Пакет присутствует в проекте | Наличие пакета не означает зависимости DialogueSystem от TMP |
| URP `17.6.0`, 2D-пакеты | Игровая сцена и графика | Окружение `vano` | Прямых импортов из них в диалоговом коде нет |
| Test Framework `1.7.0` | Установлен в проекте | Тестовая инфраструктура | Собственных тестов диалога не найдено |
| UniTask, DOTween | Не обнаружены в соответствующем коде/manifest | — | Не являются установленными требованиями текущей реализации |

## 7. Dialogue Data Model

Модель небольшая и линейная:

```text
Dialogue
  lines: DialogueLine[]

DialogueLine
  speakerKey: string
  textKey: string
  presentation: string = "standard"
```

Это `[Serializable]` C#-классы, **не ScriptableObject-диалоги**. Сейчас экземпляры находятся непосредственно в сериализованных полях `NpcDialogue` и `GameplaySceneConfig`.

Говорящий представлен только ключом строки. Нет отдельного speaker/character object, портрета, голоса или ссылки на NPC.

Текст и имя извлекаются из одной таблицы `Dialogue`. Пустой `speakerKey` даёт пустое имя; пустой ключ текста проходит в localization service, который пишет ошибку и возвращает пустую строку.

`presentation` — строковый ID, а не тип реплики. Asset задаёт:

| ID | USS class | Typing | Speaker |
|---|---|---|---|
| `standard` | `dialogue-standard` | Typewriter | Показывать |
| `system` | `dialogue-system` | Instant | Скрывать |

В NPC встречается `presentation: test`, отсутствующий в конфигурации: resolver возвращает `standard`.

Runtime-состояние включает ссылку на диалог, индексы реплики/страницы, массив строк страниц, текущую presentation и флаг активности. Оно не сохраняется.

Поддерживаемая разметка:

| Markup | Назначение |
|---|---|
| `[format=...]...[/format]` | Unity rich-text форматирование |
| `[style=...]...[/style]` | Именованный TextCore-стиль |
| `[effect=...]...[/effect]` | Wave, shake, rainbow, bounce |
| `[speed=...]...[/speed]` | Скорость печати в символах в секунду |
| `[instant]...[/instant]` | Мгновенный вывод участка |
| `[pause=...]` | Пауза в секундах |
| `[wait]` | Ожидание Submit |
| `[page]` | Явная граница страницы |
| `[event=...]` | Текстовое событие с ID |

Paginator закрывает и заново открывает scope-теги на границе страниц. В предусмотренных случаях убирает `[wait]` на границе, поскольку смена страницы уже требует Submit. Слишком большое слово принимается даже при отрицательном результате измерения; строгой гарантии отсутствия переполнения нет.

Важное различие: presentation `Instant` вызывает только rich renderer. Typewriter-команды, включая `[event]` и `[wait]`, в этом режиме не исполняются. Это отличается от `[instant]` внутри typewriter-потока.

Ветвлений, вариантов ответа, условий, переменных диалога, переходов по ID, портретов и save/load hooks не найдено. Метаданные записей текущих таблиц пустые.

Контракт локализации имеет `LocaleChanged`, но `DialogueController` на него не подписан: уже подготовленные страницы автоматически не обновляются при смене языка. Следующая реплика разрешается заново.

## 8. Unity/UI Layer

Текущее окно находится на объекте `UI` внутри `AppRoot.prefab`, рядом с `WorldUIRoot`. Самостоятельного prefab только для DialogueSystem не найдено.

`PanelRenderer` ссылается на:

- `DialogueView.uxml`;
- `PanelSettings.asset`;
- через panel settings — `DialogueTextSettings.asset`;
- через text settings — `DialogueTextStyles.asset`.

View требует точных имён элементов:

```text
dialogue-root
dialogue-speaker
dialogue-text
dialogue-continue
dialogue-text-viewport
dialogue-measure-text
```

Отсутствие любого обязательного элемента вызывает `InvalidOperationException`.

Измерительный label использует тот же текстовый класс, скрыт и расположен абсолютно. `CanFitText()` строит rich text и вызывает `MeasureTextSize` по текущей ширине viewport, сравнивая высоту с доступной.

Текущие USS-настройки: отступы окна по горизонтали 10%, viewport высотой 150 px, текст 24 px, speaker 20 px. `standard` располагает окно снизу, `system` — сверху. Индикатор продолжения — label `▼`, а не кнопка.

Окно скрывается через `visibility`, поэтому сохраняет участие в layout. Это позволяет получить геометрию до начала диалога.

Эффекты реализованы через `TextElement.PostProcessTextVertices`, glyph/link ID и `MarkDirtyRepaint()`. Отдельных Animator/AnimationClip для окна нет. Typewriter использует корутины, `Time.unscaledDeltaTime` и `WaitForSecondsRealtime`; визуальные эффекты получают `Time.unscaledTime`.

Canvas, `TMP_Text`, UI-кнопки выбора, обработчики кликов и EventSystem в цепочке управления диалогом не обнаружены. Подтверждение и отмена идут напрямую через Input System.

`DialogueTextSettings` не содержит назначенного default font asset или fallback fonts. Следовательно, текущий вид опирается на базовую тему/настройки Unity; конкретный библиотечный шрифт здесь не определён.

После UI reload view пересоздаёт текстовые системы и скрывается. Контроллер не получает уведомления о сбросе UI и не восстанавливает текущую страницу. Изменение размера окна также не запускает повторную пагинацию уже подготовленной реплики.

## 9. Tests / Temporary / Legacy Code

Автоматических EditMode/PlayMode-тестов диалога, NUnit/UnityTest и тестовых assembly definitions в пользовательских Assets не найдено. Отдельной актуальной тестовой сцены DialogueSystem также нет.

Существующие ручные проверки:

- NPC в `Gameplay.unity` содержит пять тестовых реплик.
- Таблицы en/ru проверяют форматирование, эффекты, пробелы, длинный текст, скорость, pause, wait и event.
- Первая реплика использует `system`, последняя — неизвестный presentation `test`.
- Английская таблица содержит также русскоязычные тестовые строки; это тестовый контент, а не завершённый набор переводов.
- `SpeechBubbleTest` через секунду показывает строку с wave-эффектом на пять секунд. Это ручной демонстрационный скрипт общего renderer, не тест жизненного цикла диалога.

Диагностика:

- `TextMarkupDiagnostics` вызывается только в Editor/Development Build и выводит warnings.
- Validator проверяет известность тегов, вложенность и часть аргументов, но не гарантирует существование конкретного style/effect или правильность localization key.
- Текстовые события в контроллере только логируются.
- View и gameplay scope явно требуют свои settings assets.

Legacy-признаки:

- `scripts/typewriter.txt` содержит старую реализацию вне компилируемых Assets.
- `_Recovery/0.unity` сериализует прежний `DialogueController` как компонент и старые поля view; актуальный контроллер уже обычный C#-объект.
- В view, renderer и typewriter остались закомментированные прежние реализации.
- `DefaultTypewriterTiming.asset` содержит старое поле `characterMultiplier`, отсутствующее в текущем классе.
- У компонента игрока в YAML остался устаревший `m_EditorClassIdentifier`; по GUID это `PlayerInputController`, а не второй `PlayerController`.
- На игроке также сохранён `PlayerInput` с пустыми UnityEvent callbacks. Прослеженный рабочий путь использует отдельный `new PlayerControls()` через `GameInputActions`; роль дополнительного компонента требует проверки при последующем runtime-тестировании.

## 10. Candidate Extraction Boundary

Это предварительная граница для обсуждения, без выбора структуры пакета.

**Likely reusable**

- `IDialogueSystem`, `Dialogue`, `DialogueLine`.
- Логика последовательности реплик и страниц внутри контроллера.
- `DialoguePaginator`.
- Runtime-модель presentation и правила resolver.
- Токены, нормализация, разбор и проверка markup.
- Модели команд и состояния typewriter.
- Модели форматов, стилей и эффектов.

**Likely reusable Unity layer**

- `DialogueView`, UXML и базовая USS-разметка.
- `TextRichRenderer` и glyph effect handlers.
- Корутинный `TextTypewriter`.
- ScriptableObject-настройки presentation и cadence.
- Диагностический вывод в Unity.
- Panel/text/style assets как часть готового UI или примера — принадлежность ещё не решена.

**Likely game-specific**

- `NpcDialogue`, выбор NPC и взаимодействие.
- `PlayerController`, `PlayerInputController`, `CharacterController` и движение.
- `InputModeController`.
- Конкретные `GameInputActions` / `PlayerControls`.
- `AppBootstrap`, scope приложения/сцены, загрузка Gameplay.
- Содержание диалогов, локализационные ключи и таблицы `vano`.
- Сцены, игровой root prefab и будущая story/event orchestration.

**Unclear / requires architectural decision**

- Принадлежность общего `UI/Text`, учитывая второго потребителя `SpeechBubble`.
- Включение или исключение SpeechBubble как отдельной функции.
- Место `ILocalizationService` и Unity Localization adapter.
- Формат подключения ввода: текущий adapter связан с игровыми actions.
- Lifetime контроллера относительно persistent view.
- Обязательность готовой темы и конкретных assets.

**Наименьший связный функциональный набор** — `UI/Dialogue` вместе с используемым `UI/Text`, UI-шаблоном и настройками. Для работы именно текущего кода этому набору дополнительно нужны проектные классы ввода и локализации. Поэтому сегодня он ещё не представляет собой независимую библиотеку.

## 11. Extraction Blockers

1. **Прямые зависимости контроллера на игровые классы.**  
   `DialogueInputController` и `InputModeController` находятся снаружи диалоговой папки. Второй требует `PlayerInputController`; первый — `GameInputActions` и сгенерированный `PlayerControls`.

2. **Текстовый движок расположен отдельно и используется совместно.**  
   Копирование только `UI/Dialogue` оставит отсутствующие типы parser, renderer, typewriter, diagnostics и settings. При переносе общего текста необходимо учитывать оставшийся `SpeechBubbleView`.

3. **Внешний контракт локализации и жёстко выбранная таблица.**  
   Без `ILocalizationService` и `LocalizationTables` контроллер не компилируется. Для работы текущего адаптера нужны Unity Localization, assets и Addressables configuration.

4. **Нет оформленной границы assembly.**  
   Пользовательские runtime-классы сейчас находятся в `Assembly-CSharp`, без собственных `asmdef` и namespaces. В частности, `AppBootstrap` читает `internal DialogueView.IsBootstrapped`: при разделении на сборки этот доступ станет недоступен без отдельного решения.

5. **UI зависит от сериализованной сборки приложения.**  
   Нужны `PanelRenderer`, правильный UXML, named elements, panel/text/style settings и timing asset. В текущем виде всё собрано внутри игрового `AppRoot.prefab`.

6. **Есть зависимость от конкретных UI Toolkit API.**  
   Используются `PanelRenderer.RegisterUIReloadCallback` и `TextElement.PostProcessTextVertices`. Наличие подходящего UI Toolkit в целевой версии Unity обязательно; минимальная поддерживаемая версия пока не установлена. Исходный проект и sandbox сейчас оба используют `6000.5.6f1`.

7. **Sandbox не повторяет интеграционные зависимости.**  
   В его manifest есть Input System, но нет объявленных Unity Localization и VContainer. Неизменённую текущую сборку приложения туда перенести без решения по этим зависимостям нельзя.

Следующие пункты не блокируют компиляцию сами по себе, но важны для безопасной границы:

- Нет контракта завершения, прерывания и результата диалога для внешнего оркестратора.
- `EndDialogue()` всегда возвращает Gameplay, не восстанавливая прежний режим.
- Контроллер сцены и persistent view имеют разный lifetime; `Dispose()` не закрывает UI.
- Readiness предполагается обеспеченной bootstrap; `StartDialogue()` сам её не проверяет.
- UI reload может скрыть активный диалог без сброса контроллера.
- `TextRichEffectRegistry.Default` — общий изменяемый статический объект.
- `null` внутри массива реплик не проверяется.
- Автоматического покрытия, подтверждающего сохранение поведения при извлечении, нет.

## 12. Questions for Architecture Discussion

1. Должен ли будущий DialogueSystem содержать общий текстовый движок, или этот движок должен иметь отдельную границу для Dialogue и SpeechBubble?

2. Входит ли SpeechBubble в функциональный объём библиотеки, либо остаётся внешним потребителем текста?

3. Кто будет владеть вводом: библиотека с Unity input adapter или внешний оркестратор, передающий команды продвижения и отмены? Блокировка игрока сейчас находится внутри цепочки диалога.

4. Как вызывающий код должен узнавать об окончании и отличать нормальное завершение, отмену, замену другим диалогом и выгрузку сцены?

5. Где проходит граница локализации: сохраняются ли ключи и provider, кто выбирает таблицу, обязателен ли Unity Localization adapter?

6. Какой lifetime требуется сессии и view? Допустим ли диалог через смену сцен и что должно происходить при UI reload?

7. Что считать поведением по умолчанию при повторном запуске: текущую немую замену или явно определённое прерывание/отклонение/очередь?

8. Должны ли `[event]` и `[wait]` исполняться в presentation `Instant`? Сейчас семантика отличается от typewriter и его `[instant]`.

9. Пакет должен поставлять готовое окно со стилями и assets или только механизмы подключения собственного UI? От этого зависит контракт обязательных элементов и геометрии.

10. Какую минимальную версию Unity нужно поддерживать с учётом используемых `PanelRenderer` и glyph API?

Аудит завершён. Extraction и изменения архитектуры не выполнялись.