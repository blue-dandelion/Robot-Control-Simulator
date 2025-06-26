from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from api.simulator import simulate

app = FastAPI()

# Allow expo app to call this API
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["POST"],
    allow_headers=["*"],
)

@app.get("/")
def home():
    return "Hello World"

@app.post("/process")
async def process(code: str):
    errors, outputs = simulate(code)
    return {"errors": errors, "outputs": outputs}
