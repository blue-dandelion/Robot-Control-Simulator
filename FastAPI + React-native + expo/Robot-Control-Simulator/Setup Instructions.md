# Frontend
1. Change **frontend/tsconfig.json** content to:
```json
{
  "extends": "expo/tsconfig.base.json",
  "compilerOptions": {
    "strict": true
  }
}
```

2. Right click on **frontend/App.tsx**. Select **Open in Integrated Terminal** and run:
```bash
npm install expo
npx expo install
```

3. Preview frontend on the phone:
```bash
npx expo start
```

# Backend

1. Create a Python Virtual Interpreter.

2. Right click on **backend/main.py**. Select **Open in Integrated Terminal** and run:
```bash
pip install fastapi
pip install 'uvicorn[standard]'
```

3. Get the local machine's Wireless LAN adapter Wi-Fi IPv4 Address:
```bash
ipconfig
```

4. Run **backend/main.py** to start the server.

*Make sure the front end device and the back end device are under the same Wi‑Fi network.*
