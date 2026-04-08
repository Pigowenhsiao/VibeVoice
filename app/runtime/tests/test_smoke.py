from fastapi.testclient import TestClient

from app.runtime.main import app


client = TestClient(app)


def test_health_endpoint_returns_runtime_summary():
    response = client.get("/health")
    assert response.status_code == 200
    payload = response.json()
    assert "runtime" in payload
    assert "model" in payload
    assert "runtime_version" in payload


def test_voices_endpoint_lists_demo_presets():
    response = client.get("/voices")
    assert response.status_code == 200
    payload = response.json()
    assert payload["items"]
