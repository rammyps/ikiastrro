# Design QA — Key Inference and shared header tabs

- Source visual truth: `D:\@ClaudeSpace\ikiastrro\UI_SVG_Templates\V2.1-Build\KEY-INFERENCE-TAB-DESIGN-v2.png`
- Implementation target: `http://127.0.0.1:5161/key-inference/3?step=karakas`
- Intended viewport: desktop in-app browser
- Source pixels: 1240 × 1800
- Implementation pixels: 812 × 688 browser viewport captures displayed inline in the current task; the browser surface did not expose a file path
- Density normalization: both reviewed at CSS pixel scale; comparison focused on the shared header, master-step rail and Step 4 heading because the reference depicts Step 2 content.
- State: Key Inference Step 4, person-level All Charts, and Saved Chart routes inspected.

**Findings**

- No actionable P0/P1/P2 issues remain for the requested migration or app-wide tab treatment.

**Required fidelity surfaces**

- Fonts and typography: the established Manrope hierarchy is preserved; numbered master steps use the reference's compact bold treatment.
- Spacing and layout rhythm: master steps form one horizontal, evenly spaced pill rail; the shared header uses navigation-left, brand-centre and saved-chart-right zones on every route.
- Colors and visual tokens: inactive steps and header tabs use a quiet cream tint. The master active step uses sunset orange; the active header tab uses navy for contrast against the orange bar.
- Image quality and asset fidelity: no raster assets are present or introduced; the existing code-native SVG chart is preserved.
- Copy and content: header labels read ALL / CHARTS, KEY / INFERENCE and SAVED / CHART on deliberate two-line treatments; database rows remain expressed as readable interpretations rather than technical table names.

**Primary interactions tested**

- All four master tabs render as one shared rail and Step 2 → Step 4 switching works.
- Step 4 loads the complete interactive Karaka wheel and its existing chart/Lagna controls.
- The legacy `/charts/3/karaka-wheel` route redirects to `/key-inference/3?step=karakas`.
- ALL CHARTS returns to the person-level chart gallery route.
- Key Inference, All Charts and Saved Chart routes each show the correct active header state.
- The brand line is smaller and centred; SAVED / CHART remains anchored at the extreme right.
- Browser-rendered implementation was captured and inspected successfully; no visible runtime error UI appeared.

**Comparison history**

- First migration capture showed the correct horizontal structure but inherited the nested navy/orange active styling and rectangular joined tabs.
- The master rail received dedicated MudTabs header/button classes; the second capture shows separated cream pills and a sunset-orange active pill matching the reference hierarchy.
- Post-fix browser evidence confirms Step 4 content and legacy-route redirection.
- The shared header was then converted to the same pill geometry at every route; browser evidence confirms the requested two-line labels and three-zone alignment.

**Implementation checklist**

- Keep future Key Inference steps inside the same horizontal master rail.
- Preserve nested tab styling as a distinct secondary hierarchy.
- Keep the Karaka wheel canonical inside Step 4 and redirect old bookmarks.

final result: passed
