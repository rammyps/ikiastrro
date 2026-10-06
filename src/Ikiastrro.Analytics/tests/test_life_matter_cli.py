from types import SimpleNamespace
import pytest
from ikiastrro_analytics import cli

class Connection:
    def __enter__(self): return self
    def __exit__(self, *_): return False

def test_validate_reports_small_cohort(monkeypatch, capsys):
    monkeypatch.setattr(cli, "connect", lambda: Connection())
    monkeypatch.setattr(cli, "fetch_life_matter_features", lambda _: [])
    assert cli.validate_life_matters() == 0
    output = capsys.readouterr().out
    assert "eligible_subjects=0" in output
    assert "percentiles_publishable=no" in output

def test_validate_returns_failure_for_invalid_contract(monkeypatch, capsys):
    monkeypatch.setattr(cli, "connect", lambda: Connection())
    monkeypatch.setattr(cli, "fetch_life_matter_features", lambda _: [{"bad": True}])
    assert cli.validate_life_matters() == 1
    assert "validation_error=" in capsys.readouterr().out

def test_describe_runs_in_order(monkeypatch, capsys):
    events = []
    report = SimpleNamespace(require_valid=lambda: events.append("validate"))
    monkeypatch.setattr(cli, "connect", lambda: Connection())
    monkeypatch.setattr(cli, "fetch_life_matter_features", lambda _: [{"row": 1}])
    monkeypatch.setattr(cli, "validate_life_matter_rows", lambda _: report)
    monkeypatch.setattr(cli, "ensure_life_matter_dataset",
                        lambda _, git_commit=None: events.append(("dataset", git_commit)) or 12)
    monkeypatch.setattr(cli, "build_life_matter_comparisons",
                        lambda *_args, **_kwargs: events.append("build") or [{"comparison": 1}])
    monkeypatch.setattr(cli, "publish_life_matter_comparisons",
                        lambda _, dataset_id, value, **__: events.append(("publish", dataset_id)) or 34)
    assert cli.describe_life_matters("abc123") == 0
    assert events == ["validate", ("dataset", "abc123"), "build", ("publish", 12)]
    assert "dataset_id=12 analytics_run_id=34 comparisons=1" in capsys.readouterr().out

def test_describe_stops_before_writes_on_invalid_contract(monkeypatch):
    monkeypatch.setattr(cli, "connect", lambda: Connection())
    monkeypatch.setattr(cli, "fetch_life_matter_features", lambda _: [{"bad": True}])
    monkeypatch.setattr(cli, "ensure_life_matter_dataset",
                        lambda *_args, **_kwargs: pytest.fail("dataset write must not run"))
    with pytest.raises(ValueError, match="Invalid analytics dataset"):
        cli.describe_life_matters(None)
