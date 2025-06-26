from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from api.simulator import simulate
from pydantic import BaseModel

app = FastAPI()

# Allow expo app to call this API
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["POST"],
    allow_headers=["*"],
)

class ControlCode(BaseModel):
    text: str

@app.get("/")
def home():
    return "Hello World"

@app.post("/process")
async def process(code: ControlCode):
    error_list, outputs_list = simulate(code.text)
    errors = '\n'.join(error_list)
    outputs = '\n'.join(outputs_list)
    return {"errors": errors, "outputs": outputs}
