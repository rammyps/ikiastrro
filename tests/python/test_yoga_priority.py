import importlib.util
import pathlib
import sys
import unittest

MODULE_PATH = pathlib.Path(__file__).parents[2] / "scripts" / "yoga_priority.py"
SPEC = importlib.util.spec_from_file_location("yoga_priority", MODULE_PATH)
MODULE = importlib.util.module_from_spec(SPEC)
assert SPEC.loader
sys.modules[SPEC.name] = MODULE
SPEC.loader.exec_module(MODULE)


class YogaPriorityTests(unittest.TestCase):
    def test_wilson_interval_is_bounded(self):
        lower, upper = MODULE.wilson(30, 100)
        self.assertLess(lower, 0.30)
        self.assertGreater(upper, 0.30)
        self.assertGreaterEqual(lower, 0)
        self.assertLessEqual(upper, 1)

    def test_missing_interpretation_increases_research_priority(self):
        common = dict(source_ref_code="SRC_TEST", yoga_set_name="Raja", evaluation_status="EVALUATED",
                      formation_rule="lords of angle and trine join", evaluated_count=100, present_count=20)
        rows = [
            MODULE.YogaRow(source_variant_code="TEST_1", yoga_code="YOGA_ONE", **common),
            MODULE.YogaRow(source_variant_code="TEST_2", yoga_code="YOGA_TWO",
                           standard_text="Meaning", short_text="Short", **common),
        ]
        ranked = {row["source_variant_code"]: row for row in MODULE.rank(rows)}
        self.assertGreater(ranked["TEST_1"]["research_priority_score"], ranked["TEST_2"]["research_priority_score"])

    def test_semantically_related_rows_share_cluster(self):
        rows = [
            MODULE.YogaRow("SRC_A", "A", "YOGA_DHANA", formation_rule="second lord wealth gain"),
            MODULE.YogaRow("SRC_B", "B", "YOGA_DHANA", formation_rule="second lord wealth gain"),
            MODULE.YogaRow("SRC_A", "C", "YOGA_ROGA", formation_rule="sixth lord disease"),
        ]
        ranked = {row["source_variant_code"]: row for row in MODULE.rank(rows)}
        self.assertEqual(ranked["A"]["semantic_cluster"], ranked["B"]["semantic_cluster"])
        self.assertNotEqual(ranked["A"]["semantic_cluster"], ranked["C"]["semantic_cluster"])


if __name__ == "__main__":
    unittest.main()
