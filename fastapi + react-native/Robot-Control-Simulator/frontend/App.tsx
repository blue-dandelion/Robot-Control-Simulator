import { Styles } from './constants/styles';
import { StatusBar } from 'expo-status-bar';
import { Alert, Button, Pressable, ScrollView, Text, TextInput, View } from 'react-native';
import ThemedView from './components/ThemedView';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { useState } from 'react';

export default function App() {

  const [code, setCode] = useState('')
  const [errors, setErrors] = useState('')
  const [outputs, setOutputs] = useState('')

  const runCode = async () => {
    try {
      // machine's LAN IP
      const BASE_URL = 'http://localhost:8000'

      const result = await fetch(`${BASE_URL}/process`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ text: code })
        }
      );

      if (!result.ok) throw new Error(`HTTP ${result.status}`);
      const json = await result.json();
      setErrors("lala")
      setOutputs(json.outputs)
    }
    catch (err) {
      if (err instanceof Error)
        Alert.alert('Error', err.message)
    }
  }

  return (
    <SafeAreaProvider>
      <ThemedView safe={true} style={{ flex: 1, flexDirection: 'column' }}>
        <View style={{ flexDirection: 'row', alignItems: 'flex-start', width: '100%', height: 'auto', backgroundColor: '#ccc' }}>
          <Button title='Run' color="#841584" onPress={runCode} />
          <Button title='Stop' color="#841584" />
        </View>

        <ScrollView>
          {/* Preview */}
          <View style={{ backgroundColor: '#c22', height: 400 }}>

          </View>

          {/* IDE */}
          <View style={{ alignItems: 'flex-start', height: 'auto', backgroundColor: '#c1c' }}>
            <Text style={Styles.title}>Code</Text>
            <ScrollView style={{ width: '100%', height: 200 }}>
              <TextInput style={{ width: '100%', minHeight: 200, backgroundColor: '#ccc' }} value={code} multiline={true} onChangeText={setCode} />
            </ScrollView>
          </View>


          {/* Error */}
          <View style={{ alignItems: 'flex-start', width: '100%', height: 'auto', backgroundColor: '#11c' }}>
            <Text style={Styles.title}>Error</Text>
            <ScrollView style={{ width: '100%', height: 100 }}>
              <TextInput style={{ width: '100%', minHeight: 100, backgroundColor: '#ccc' }} value={errors} multiline={true} onChangeText={setErrors} />
            </ScrollView>
          </View>

          {/* Output */}
          <View style={{ alignItems: 'flex-start', width: '100%', height: 'auto', backgroundColor: '#11c' }}>
            <Text style={Styles.title}>Output</Text>
            <ScrollView style={{ width: '100%', height: 100 }}>
              <TextInput style={{ width: '100%', minHeight: 100, backgroundColor: '#ccc' }} value={outputs} multiline={true} onChangeText={setOutputs} />
            </ScrollView>
          </View>

          <View style={{ height: 500 }} />
        </ScrollView>

      </ThemedView>

    </SafeAreaProvider>

  );
}

