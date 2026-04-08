from app.runtime.main import app, create_app


if __name__ == "__main__":
    import uvicorn

    uvicorn.run("app.runtime.main:app", host="127.0.0.1", port=8765, reload=False)
