# Local model integration

No models are downloaded or loaded by this scaffold. Add an explicit versioned model manifest with upstream identity, license, size, and integrity hash when choosing assets.

Implement IInferenceProvider for local inference. ONNX Runtime for C# is a candidate; select runtime packages and hardware execution providers only after testing representative devices.

Keep OCR and other vision work bounded, cancel stale results, and avoid retaining screenshots or private task context as reusable knowledge.
