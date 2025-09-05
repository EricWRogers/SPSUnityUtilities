# SPSUnityUtilities
SuperPupStudio Unity Utilities

## naming  convention

- class name noun PascalCase
- public class var camelCase
- private class var m_camelCase
- function PascalCase
- method PascalCase
- parameters _camelCase

## documenting code

### editor
- [Header("")]
- [Tooltip("")]

### public functions/methods recommended
> /// \<summary> </br>
> /// Start timer will start the timer with the values passed in or</br>
> /// the public class variables are null.</br>
> /// \</summary></br>
> /// \<param name="_countDown">The amount of time in seconds the timer will run.\</param></br>
> /// \<param name="_autoRestart">If true the timer will restart when finish.\</param></br>
> /// \<return>message about return\</return></br>

## commit guideline

### message
[type][ticket][message]

Type
- [DOC] Documentation
- [FIX] Bug Fix
- [FEATURE]
- [BUILD] Build/Dependencies

### branch
lower-case with - used instead of space
