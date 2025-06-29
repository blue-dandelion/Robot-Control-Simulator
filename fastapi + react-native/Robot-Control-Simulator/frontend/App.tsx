import { Styles } from './constants/styles';
import { Alert, Button, Pressable, ScrollView, Text, TextInput, useColorScheme, View } from 'react-native';
import ThemedView from './components/ThemedView';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { useEffect, useRef, useState } from 'react';
import ThemedButton from './components/ThemedButton';
import { Colors } from './constants/colors';
import ThemedText from './components/ThemedText';
import DecoLine from './components/DecoLine';
import WorkspacePreview, { WorkspacePreviewHandles } from './components/WorkspacePreview';
import { Direction, RotateTo } from './constants/deps';
import ThemedTextInput from './components/ThemedTextInput';

export default function App() {
  const colorScheme = useColorScheme()
  const theme = colorScheme ? Colors[colorScheme] : Colors.light

  const workspacePreviewRef = useRef<WorkspacePreviewHandles>(null)

  const [serverURL, setServerURL] = useState('');
  const [connected, setConnected] = useState(false);
  const [code, setCode] = useState('');
  const [errors, setErrors] = useState('');
  const [outputs, setOutputs] = useState('');
  const [console, setConsole] = useState('');
  const ws = useRef<WebSocket | null>(null);

  const connect = () => {
    if (ws.current) return;

    const IPCONFIG = '192.168.1.206'
    const URL = `ws://${IPCONFIG}:8080/process`
    ws.current = new WebSocket(URL);

    setConsole(prev => prev + `Connecting to server ${URL}...\n`)

    ws.current.onopen = () => {
      setConnected(true);
      setConsole(prev => prev + `Connect successfully\n`)
    }

    ws.current.onmessage = (e) => {
      try {
        // Read the received message
        const data = JSON.parse(e.data);
        let type = data.type;

        if (type == "warning") {
          setErrors(prev => prev + data.content + '\n')
        }
        else if (type == "PLACE") {
          workspacePreviewRef.current?.place(data.content.x, data.content.y, parseInt(data.content.f, 10))
        }
        else if (type == "MOVE") {
          workspacePreviewRef.current?.move(data.content.x, data.content.y)
        }
        else if (type == "ROTATE") {
          workspacePreviewRef.current?.rotate(parseInt(data.content, 10))
        }
        else if (type == "REPORT") {
          setOutputs(prev => prev + data.content + '\n')
        }
      }
      catch (err) {
        if (err instanceof Error)
          Alert.alert('Error', err.message)
      }
    }

    ws.current.onerror = (e) => {
      Alert.alert('Websocket Error', e.type)
    }

    ws.current.onclose = () => {
      setConnected(false);
      setConsole(prev => prev + `Websocket closed ${URL}\n`)
    }
  }

  const disconnect = () => {
    if (ws.current) {
      ws.current.close();
      ws.current = null;
    }
  }

  const run = () => {
    if (ws.current && connected) {
      ws.current.send(code);
    }
  }

  // const runCode = async () => {
  //   try {
  //     // machine's LAN IP
  //     const BASE_URL = 'http://192.168.1.206:8000'

  //     const result = await fetch(`${BASE_URL}/process`,
  //       {
  //         method: 'POST',
  //         headers: { 'Content-Type': 'application/json' },
  //         body: JSON.stringify({ text: code })
  //       }
  //     );

  //     if (!result.ok) throw new Error(`HTTP ${result.status}`);
  //     const json = await result.json();
  //     setErrors(json.errors)
  //     setOutputs(json.outputs)
  //   }
  //   catch (err) {
  //     if (err instanceof Error)
  //       Alert.alert('Error', err.message)
  //   }
  // }

  return (
    <SafeAreaProvider>
      <ThemedView safe={true} style={{ flex: 1, flexDirection: 'column' }}>
        <View style={{ flexDirection: 'row', justifyContent: 'center', width: '100%', height: 'auto', padding: 10, backgroundColor: theme.background_tl }}>
          <ThemedTextInput style={{ width: 200, height: 30, marginHorizontal: 10 }} alignSelf='center' value={serverURL} onChangeText={setServerURL} />
          <ThemedButton text={connected ? "Disconnect" : "Connect"} onPress={connected ? disconnect : connect} />
        </View>
        <View style={{ flexDirection: 'row', justifyContent: 'center', width: '100%', height: 'auto', padding: 10, backgroundColor: theme.background_tl }}>

          <ThemedTextInput style={{ width: 50, height: 30, marginHorizontal: 10 }} alignSelf='center' value={serverURL} onChangeText={setServerURL} />

          <ThemedButton text='Run' onPress={run} />
        </View>
        <DecoLine direction='Horizontal' />

        <ScrollView>
          {/* Preview */}
          <View style={{ backgroundColor: '#000', height: 400, alignItems: 'center', justifyContent: 'center' }}>
            <WorkspacePreview ref={workspacePreviewRef} />
          </View>

          {/* IDE */}
          <View style={{ alignItems: 'flex-start', height: 'auto', paddingTop: 10 }}>
            <ThemedText title={true} style={{ marginLeft: 10, marginBottom: 5 }}>Code</ThemedText>
            <ScrollView style={{ width: '100%', height: 200, backgroundColor: theme.background_tl }}>
              <TextInput style={{ width: '100%', minHeight: 200, marginHorizontal: 10, fontSize: 16, color: theme.text }} value={code} multiline={true} onChangeText={setCode} />
            </ScrollView>
          </View>

          {/* Error */}
          <View style={{ alignItems: 'flex-start', width: '100%', height: 'auto', paddingTop: 10 }}>
            <ThemedText title={true} style={{ marginLeft: 10, marginBottom: 5 }}>Error</ThemedText>
            <ScrollView style={{ width: '100%', height: 100, backgroundColor: theme.background_tl }}>
              <TextInput style={{ width: '100%', minHeight: 100, marginHorizontal: 10, fontSize: 16, color: theme.text }} editable={false} value={errors} multiline={true} onChangeText={setErrors} />
            </ScrollView>
          </View>

          {/* Output */}
          <View style={{ alignItems: 'flex-start', width: '100%', height: 'auto', paddingTop: 10 }}>
            <ThemedText title={true} style={{ marginLeft: 10, marginBottom: 5 }}>Output</ThemedText>
            <ScrollView style={{ width: '100%', height: 100, backgroundColor: theme.background_tl }}>
              <TextInput style={{ width: '100%', minHeight: 100, marginHorizontal: 10, fontSize: 16, color: theme.text }} editable={false} value={outputs} multiline={true} onChangeText={setOutputs} />
            </ScrollView>
          </View>

          {/* Console */}
          <View style={{ alignItems: 'flex-start', width: '100%', height: 'auto', paddingTop: 10 }}>
            <ThemedText title={true} style={{ marginLeft: 10, marginBottom: 5 }}>Console</ThemedText>
            <ScrollView style={{ width: '100%', height: 100, backgroundColor: theme.background_tl }}>
              <TextInput style={{ width: '100%', minHeight: 100, marginHorizontal: 10, fontSize: 16, color: theme.text }} editable={false} value={console} multiline={true} onChangeText={setConsole} />
            </ScrollView>
          </View>

          <View style={{ height: 500 }} />
        </ScrollView>

      </ThemedView >

    </SafeAreaProvider >

  );
}

