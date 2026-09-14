# Design QA — Karaka Wheel corrections

- Source visual truth: `C:\Users\rammy\AppData\Local\Temp\codex-clipboard-4eb5b0d9-587e-48b6-b6b0-238ab0f35c53.png`
- Implementation target: `http://127.0.0.1:5161/charts/3/karaka-wheel`
- Intended viewport: desktop in-app browser
- Source pixels: 1920 × 1445 before chat resizing
- Implementation pixels: browser viewport capture displayed inline in the current task; the browser surface did not expose a file path
- State: D1 selected; House 1/Sun and House 4/Moon inspected, including lord-chain summary and question indicators

**Findings**

- No actionable P0/P1/P2 issues remain for the requested contextual-question behavior.

**Required fidelity surfaces**

- Fonts and typography: existing product typography and hierarchy preserved; question cards use the established compact reading-panel scale.
- Spacing and layout rhythm: existing wheel layout preserved; detail column widened from 360px to 390px for the life-matter reference rows.
- Colors and visual tokens: existing midnight, sunset, ivory, and muted tokens preserved.
- Image quality and asset fidelity: no raster assets are present or introduced; the existing code-native SVG chart is preserved.
- Copy and content: technical table names are removed. Database rows are expressed as readable questions filtered by chart, house, natural planet, or Chara role.

**Primary interactions tested**

- Twelve selectable house sectors render and expose accessible planet/role controls.
- Chara markers and rows render for D9 and are suppressed for an unsupported D10 state.
- D1 House 1/Sun shows questions for physical constitution and general health.
- Clicking D1 House 4/Moon updates the selection and shows the peace-of-mind question only.
- The top summary resolves house lord placement, Rāśi lord, Nakshatra/lord, and KP sub-lord; no Razor placeholder text remains.
- Two concentric rings appear only on planet markers with matching contextual questions, alongside the existing selected highlight.
- Browser-rendered implementation was captured and inspected successfully.

**Comparison history**

- Initial code pass exposed the literal `H@h` placeholder and unrestricted Chara rendering.
- Post-fix component snapshot proves H1–H12 output and the D9 state; focused tests pass 3/3.
- The first follow-up exposed table metadata instead of user value; the disclosure was removed and replaced with contextual prompts.
- Post-fix browser evidence confirms chart + house + Karaka filtering and readable question copy.

**Implementation checklist**

- Keep question wording derived from `MatterText` and references derived from `HouseFromKarakaText`.
- Preserve natural-planet and Chara-role selection as the filter input.
- Keep database table names out of the user interface.

final result: passed
