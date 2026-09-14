# Design QA — Key Inference Karakas migration

- Source visual truth: `D:\@ClaudeSpace\ikiastrro\UI_SVG_Templates\V2.1-Build\KEY-INFERENCE-TAB-DESIGN-v2.png`
- Implementation target: `http://127.0.0.1:5161/key-inference/3?step=karakas`
- Intended viewport: desktop in-app browser
- Source pixels: 1240 × 1800
- Implementation pixels: 812 × 688 browser viewport capture displayed inline in the current task; the browser surface did not expose a file path
- Density normalization: both reviewed at CSS pixel scale; comparison focused on the shared header, master-step rail and Step 4 heading because the reference depicts Step 2 content.
- State: Step 4 selected; D1 selected; House 1/Sun active.

**Findings**

- No actionable P0/P1/P2 issues remain for the requested migration or shared tab treatment.

**Required fidelity surfaces**

- Fonts and typography: the established Manrope hierarchy is preserved; numbered master steps use the reference's compact bold treatment.
- Spacing and layout rhythm: master steps form one horizontal, evenly spaced pill rail above every Key Inference panel, matching the reference hierarchy.
- Colors and visual tokens: inactive steps use a quiet cream tint and the active step uses sunset orange, while nested tabs retain the established navy/orange convention.
- Image quality and asset fidelity: no raster assets are present or introduced; the existing code-native SVG chart is preserved.
- Copy and content: the global navigation reads ALL CHARTS; the new master step reads 4. KARAKAS; database rows remain expressed as readable interpretations rather than technical table names.

**Primary interactions tested**

- All four master tabs render as one shared rail and Step 2 → Step 4 switching works.
- Step 4 loads the complete interactive Karaka wheel and its existing chart/Lagna controls.
- The legacy `/charts/3/karaka-wheel` route redirects to `/key-inference/3?step=karakas`.
- ALL CHARTS returns to the person-level chart gallery route.
- Browser-rendered implementation was captured and inspected successfully; no visible runtime error UI appeared.

**Comparison history**

- First migration capture showed the correct horizontal structure but inherited the nested navy/orange active styling and rectangular joined tabs.
- The master rail received dedicated MudTabs header/button classes; the second capture shows separated cream pills and a sunset-orange active pill matching the reference hierarchy.
- Post-fix browser evidence confirms Step 4 content and legacy-route redirection.

**Implementation checklist**

- Keep future Key Inference steps inside the same horizontal master rail.
- Preserve nested tab styling as a distinct secondary hierarchy.
- Keep the Karaka wheel canonical inside Step 4 and redirect old bookmarks.

final result: passed
