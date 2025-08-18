# AI Model Strategy

This document describes the AI model architecture and strategy for **ShadowrunGM**, including local and cloud models, embeddings, and orchestration.

---

## Local Models (Ollama / GPU-hosted)

Local models are used for **low-latency inference**, **offline play**, and **cost control**. They primarily support **text generation** and **embeddings**.

### Text Generation Models

* **llama3.1** (8B, 70B when VRAM permits)
* **mistral-7B** (balanced quality and speed)
* **qwen2-7B** (multilingual capability)
* **phi3-mini** (lightweight, fast inference)

### Embedding Models

* **nomic-embed-text**
* **snowflake-arctic-embed**

### Vector DB / RAG

* **LiteDB/SQLite** for local development
* **PostgreSQL + PGVector** for server deployments
* RAG retrieves relevant catalog items, rules, and campaign notes for LLM context

---

## Cloud Models (Fallback / Advanced Reasoning)

When local models lack reasoning depth or long context handling, we fall back to cloud APIs.

* **OpenAI**

  * GPT-4o → reasoning, creativity
  * GPT-5 → structured planning and complex multi-step reasoning
* **Anthropic**

  * Claude 3.5 → long context, structured reasoning
* **Azure / AWS**

  * Enterprise hosting, compliance requirements

---

## Orchestration Strategy

* **Hybrid Use**

  * Local models handle RAG lookups and lightweight narrative/dialogue.
  * Cloud models handle advanced reasoning, multi-turn storytelling, and campaign orchestration.
* **Failover**

  * If a local model fails or response quality drops, orchestrator retries with a cloud provider.
* **Benchmarks**

  * Compare models by latency, token cost, and GM accuracy.

---

## Future Expansion

* **Benchmarking Harness** → Automated A/B tests across local vs. cloud models.
* **Model Mix Strategy** → Blending outputs (local for catalog lookups, cloud for narrative flair).
* **Fine-tuning / LoRA** → Custom tuning on Shadowrun lore and SR6 rules.
* **Content Rating Enforcement** → AI filters based on user-selected maturity levels.

---

## Next Steps

* Stand up Ollama locally with `llama3.1` + embeddings.
* Validate RAG pipeline with **LiteDB → PGVector** migration path.
* Define test prompts for **GM assistant** vs **Overseer AI**.
* Add benchmarking scripts for latency and cost tracking.
