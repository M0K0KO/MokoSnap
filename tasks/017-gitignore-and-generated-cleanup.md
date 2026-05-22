\# Task: Gitignore and generated file cleanup



\## Goal



Clean up generated files and protect the repository from accidentally committing build, IDE, and publish outputs.



This task should not implement new MokoSnap features.



\## Scope



Allowed changes:



\- .gitignore

\- scripts/

\- docs/RELEASE.md only if needed

\- tasks/017-gitignore-and-generated-cleanup.md if needed



Forbidden changes:



\- Do not implement new app features.

\- Do not refactor source code.

\- Do not modify WPF UI.

\- Do not modify Chrome capture behavior.

\- Do not modify hotkey behavior.

\- Do not modify startup behavior unless required only for documentation.

\- Do not delete source files.

\- Do not delete task markdown files.



\## Assumptions



\- Visual Studio, WPF, .NET, and publish outputs generate files that should not be committed.

\- Build outputs should be ignored.

\- IDE files should be ignored.

\- Local publish output should be ignored.

\- User-specific files should be ignored.

\- Source files, docs, tasks, scripts, extension files, and project files should remain trackable.



If an assumption is wrong, stop and report it.



\## Implementation Requirements



1\. Add or update root `.gitignore`.

2\. Ignore Visual Studio files:

&#x20;  - `.vs/`

&#x20;  - `\*.user`

&#x20;  - `\*.suo`

&#x20;  - `\*.userosscache`

&#x20;  - `\*.sln.docstates`

3\. Ignore .NET build outputs:

&#x20;  - `bin/`

&#x20;  - `obj/`

&#x20;  - `TestResults/`

4\. Ignore publish/build artifacts:

&#x20;  - `artifacts/`

&#x20;  - `publish/`

5\. Ignore logs and temporary files:

&#x20;  - `\*.log`

&#x20;  - `\*.tmp`

&#x20;  - `\*.cache`

6\. Ignore local Chrome native host manifest if it exists:

&#x20;  - `extension/chrome/native-host-manifest.local.json`

7\. Keep template files trackable:

&#x20;  - do not ignore `extension/chrome/native-host-manifest.template.json`

8\. Check whether generated files are already tracked.

9\. If generated files are tracked, remove them from git index only, not from disk, using `git rm --cached` where appropriate.

10\. Do not remove source files.

11\. Run verification.



\## Verification



Run:



\- `./scripts/verify.ps1`

\- `./scripts/publish-local.ps1`



Manual check:



1\. Run `git status`.

2\. Confirm generated folders like `.vs/`, `bin/`, `obj/`, and `artifacts/` do not appear as untracked files.

3\. Confirm source changes from task 016 still appear.

4\. Confirm `.gitignore` appears as changed.

5\. Confirm `extension/chrome/native-host-manifest.template.json` is still trackable.

6\. Confirm `extension/chrome/native-host-manifest.local.json` is ignored if present.



\## Done Criteria



\- Code builds.

\- Tests pass.

\- Publish script still works.

\- Generated files are ignored.

\- No source files are accidentally deleted.

\- No unrelated files changed.

\- Final response lists changed files.

\- Final response lists verification result.

\- Final response lists known limitations.

