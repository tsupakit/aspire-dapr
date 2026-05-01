from fastapi import FastAPI

app = FastAPI(title="Aspire Dapr Python Uvicorn App")


@app.get("/")
async def root():
    return {"message": "Hello from python-app2 via Uvicorn!"}


@app.get("/health")
async def health():
    return {"status": "healthy"}


if __name__ == "__main__":
    import uvicorn

    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
