\# Task: Fix popup focus and activation



\## Goal



Fix focus and activation problems for keyboard-driven popups.



When MokoSnap opens the Quick Switcher or Close Selected Windows dialog, the popup must become the active foreground window and keyboard focus must land on the intended control.



This is a UX bugfix task.



\## Scope



Allowed changes:



\- src/MokoSnap.App/

\- src/MokoSnap.Platform.Windows/ only if foreground activation helper is needed

\- src/MokoSnap.Tests/ only if useful

\- docs/DECISIONS.md only if needed



Forbidden changes:



\- Do not implement new features.

\- Do not change hotkey behavior except focus/activation.

\- Do not change preset run behavior.

\- Do not change visible window closing behavior.

\- Do not implement Chrome tab capture.

\- Do not add third-party UI libraries.



\## Problem



Currently, when a hotkey opens a popup, the popup may appear visually but not receive keyboard focus.



Affected UI:



1\. Quick Switcher / command palette

2\. Close Selected Windows popup

3\. Any other modal popup opened during preset run if applicable



\## Requirements



\### Quick Switcher



1\. When opened by global hotkey, the Quick Switcher window must become active.

2\. The search TextBox must receive keyboard focus automatically.

3\. User must be able to type immediately after the popup appears.

4\. Enter must run the selected preset.

5\. Esc must close the popup.

6\. Up/Down must move selection.

7\. The popup should appear centered and on top of the current desktop temporarily if needed.

8\. Avoid permanently setting Topmost=true.



\### Close Selected Windows Dialog



1\. When shown, the dialog must become active.

2\. It must be owned by the main MokoSnap window when possible.

3\. If opened while MokoSnap is not foreground, it must still activate reliably.

4\. Keyboard focus should land on the candidate list or the primary action button.

5\. Esc should cancel.

6\. Enter should confirm if safe and consistent with existing UI.

7\. Space should toggle focused checkbox if focus is on the list.



\### Implementation Guidance



Use a small helper if needed, for example:



\- WindowActivationService

\- FocusFirstControlBehavior

\- DialogFocusHelper



Use WPF APIs first:



\- Window.Activate()

\- Window.Focus()

\- FrameworkElement.Focus()

\- Keyboard.Focus(...)

\- Dispatcher.BeginInvoke(..., DispatcherPriority.ApplicationIdle)



If necessary for global hotkey activation, use a minimal Win32 helper:



\- SetForegroundWindow

\- BringWindowToTop



Do not overengineer this.



\### Verification



Run:



```powershell

./scripts/verify.ps1

