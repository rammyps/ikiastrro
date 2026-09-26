import importlib.util
import pathlib
import sys
import unittest

PATH = pathlib.Path(__file__).parents[2] / "scripts" / "yoga_lifematter_matrix.py"
SPEC = importlib.util.spec_from_file_location("yoga_lifematter_matrix", PATH)
MODULE = importlib.util.module_from_spec(SPEC)
assert SPEC.loader
sys.modules[SPEC.name] = MODULE
SPEC.loader.exec_module(MODULE)


class YogaLifeMatterMatrixTests(unittest.TestCase):
    def test_generates_seven_ranked_paths(self):
        yoga = [{"yoga_variant_id": 1, "rule_set_id": 1, "source_ref_code": "SRC",
                 "source_variant_code": "Y1", "yoga_code": "YOGA_DHANA", "yoga_set_code": "DHANA",
                 "yoga_set_name": "Dhana", "formation_rule": "wealth gain", "notes": ""}]
        foci = [{"focus_id": i, "rule_set_id": 1, "life_matter_id": i, "focus_priority": 1,
                 "area": "Wealth", "sub_area1": f"Wealth {i}", "sub_area2": "Natal promise",
                 "sub_area3": "House", "sub_area4": "LAGNA", "sub_area5": "House 2",
                 "sub_area6": "Jupiter"} for i in range(1, 10)]
        rows = MODULE.generate(yoga, foci)
        self.assertEqual(7, len(rows))
        self.assertEqual(list(range(1, 8)), [row["path_rank"] for row in rows])
        self.assertEqual(7, len({row["focus_id"] for row in rows}))

    def test_dhana_prefers_wealth_semantics(self):
        yoga = [{"yoga_variant_id": 1, "rule_set_id": 1, "source_ref_code": "SRC",
                 "source_variant_code": "Y1", "yoga_code": "YOGA_DHANA", "yoga_set_code": "DHANA",
                 "yoga_set_name": "Dhana wealth", "formation_rule": "wealth income", "notes": ""}]
        foci = [
            {"focus_id": 1, "rule_set_id": 1, "life_matter_id": 1, "focus_priority": 1,
             "area": "Wealth", "sub_area1": "Income and prosperity", "sub_area2": "Natal promise",
             "sub_area3": "House", "sub_area4": "LAGNA", "sub_area5": "House 2", "sub_area6": "Jupiter"},
            {"focus_id": 2, "rule_set_id": 1, "life_matter_id": 2, "focus_priority": 1,
             "area": "Health", "sub_area1": "Disease and injury", "sub_area2": "Natal promise",
             "sub_area3": "House", "sub_area4": "LAGNA", "sub_area5": "House 6", "sub_area6": "Saturn"},
        ]
        rows = MODULE.generate(yoga, foci)
        self.assertEqual(1, rows[0]["focus_id"])


if __name__ == "__main__":
    unittest.main()
