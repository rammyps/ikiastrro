---
last_updated: 2026-09-10
---

# Nakshatras — Taittiriya Brahmana source layer

The first primary-text nakshatra layer is `SRC_TB_1_5_1_NAKSHATRAS`: Taittiriya Brahmana
1.5.1.1–5, printed pages 34–35 of the selected Sanskrit Documents edition. It provides the
deity and paired “above/below” formulation for all 27 standard nakshatras. Migration
`db/072_seed_taittiriya_brahmana_nakshatra_text.sql` stores the exact accentless Devanagari
passage and a deliberately literal project English translation for each one.

Several names in this Vedic passage differ from the later standard labels: Invaka,
Tishya, Nishtya, Shrona, Shravishtha, Proshthapada, Ashvayuj and Apabharani are retained
inside the source text. Their database rows map them to Mrigashirsha, Pushya, Swati,
Shravana, Dhanishtha, Bhadrapada, Ashwini and Bharani respectively. This is a crosswalk,
not a silent alteration of the source.

The local file `D:\Vedic Astrology\Vedic Astology Books\The Shaktis of the Nakshatras.doc`
was useful for identifying Taittiriya Brahmana 1.5.1 and the Bhattabhaskara Mishra commentary,
but it is a modern secondary article. Its prose is not copied into the research corpus.
Likewise, `The 27 Nakshatras.doc` and the local nakshatra PDFs remain secondary discovery
sources until an original Sanskrit passage, edition and printed page are independently checked.

This layer should not yet be promoted into production interpretation rules. Ambiguous Vedic
terms and the archaic-name mappings need a commentary-level review before derived attributes
or predictive meanings are asserted.
