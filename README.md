![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=POSTECH-SOAT-SALA11_application-avalanches-pagamento-ms&metric=alert_status)
![Bugs](https://sonarcloud.io/api/project_badges/measure?project=POSTECH-SOAT-SALA11_application-avalanches-pagamento-ms&metric=bugs)
![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=POSTECH-SOAT-SALA11_application-avalanches-pagamento-ms&metric=code_smells)
![Coverage](https://sonarcloud.io/api/project_badges/measure?project=POSTECH-SOAT-SALA11_application-avalanches-pagamento-ms&metric=coverage)
# Documentação do Microserviço de Processamento de Vídeos: ms-framevideo-app

Este documento descreve o fluxo de funcionamento do microserviço responsável pelo processamento de chunks disponibilizados pelo processo anterior. Ele aborda as etapas necessárias para o desenvolvimento, incluindo integrações, papéis no sistema e detalhamento técnico das ações esperadas.

---

## Visão Geral do Sistema

O sistema é um gerador de frames a partir de vídeos enviados pelos usuários. Após o upload de um vídeo no portal, o sistema processa o arquivo e retorna um `.zip` contendo imagens extraídas a cada segundo do vídeo. 

Este microserviço desempenha um papel fundamental na etapa inicial de processamento, dividindo os vídeos em pequenos pedaços (chunks) de até 1 minuto cada, que serão utilizados em fluxos subsequentes.

---

## Objetivo do Microserviço

O microserviço é responsável por:

1. Consumir mensagens de uma fila no **RabbitMQ** para identificar quando um novo chunk foi enviado ao sistema.
2. Fazer o download de um ou mais chunks de um **Bucket S3**.
3. Gerar frames a cada 1 segundo, no formato de imagem, para cada chunk.
4. Compactar frames em um arquivo ´.zip´.
5. Atualizar o banco de dados com informações dos frames gerados.
6. Publicar arquivo ´.zip´ com frames no **Bucket S3**.
7. Publicar mensagens no **RabbitMQ** informando que cada ´.zip´ contendo os frames foi gerado e armazenado.

---

## Fluxo de Funcionamento

1. **Recepção de Mensagem**
   - O microserviço se conecta a uma fila no RabbitMQ chamada 'queue-uploaded-chunk' (dentro do mesmo cluster Kubernetes em execução no EKS da AWS).
   - Ele consome mensagens que indicam que um vídeo foi enviado para o Bucket S3 'uploaded-video-chunk'.

2. **Processamento de Chunk**
   - Após receber a mensagem, o microserviço:
     - Faz o download do vídeo do Bucket S3 'uploaded-video-chunk'.
     - Divide cada chunk em frames de 1 segundo cada.

3. **Ações para Cada Frame**
   - Para cada frame gerado:
     - Compacta os frames de um respectivo chunk em um arquivo ´.zip´ nomeado pelo nome do video + chunk. 
     - Faz o upload do chunk para o Bucket S3 'uploaded-chunk-frame-bucket'.
     - Atualiza o banco de dados 'framevideo-redis-db'(**Redis**, hospedado no ElastiCache) com informações como:
       - Identificador do vídeo original.
       - Identificadores do chunks original.
       - Quantidade de frames.
       - Destinos dos frames no Bucket S3.
     - Publica uma mensagem na fila RabbitMQ 'frame-chunk-process' informando que o chunk foi processado e está disponível.

---

## Integrações e Dependências

### 1. **RabbitMQ**
   - Consumir mensagens da fila indicando novos vídeos para processamento.
   - Publicar mensagens na fila indicando que os chunks foram gerados.

### 2. **Amazon S3**
   - Download do vídeo original a partir de um Bucket específico.
   - Upload dos chunks gerados para outro Bucket.

### 3. **Banco de Dados (REDIS)**
   - Registro das informações sobre os frames gerados:
     - Quantidade.
     - Identificadores únicos.
     - Destinos.

---

## Tecnologias e Ferramentas Utilizadas

- **Linguagem e Framework**: .NET Core (C#)
- **Orquestração de Contêineres**: Kubernetes (AWS EKS)
- **Banco de Dados**: Redis (AWS ElastiCache)
- **Mensageria**: RabbitMQ
- **Armazenamento**: AWS S3

---

## Regras de Negócio e Pontos Críticos

1. **Durabilidade dos Frames**
   - Cada frame representa 1 segundo.

2. **Ordem das Operações**
   - Garantir que o upload no S3, a gravação no banco de dados e o envio de mensagens ao RabbitMQ sejam executados de forma síncrona ou com estratégias de rollback em caso de falhas.

3. **Mensagens no RabbitMQ**
   - As mensagens enviadas para o RabbitMQ devem conter:
     - Identificador do chunk.
     - Localização no S3.
     - Informações relevantes para o próximo serviço no pipeline.

4. **Gerenciamento de Erros**
   - Implementar tolerância a falhas para operações no S3, RabbitMQ e banco de dados.
   - Retentar operações falhas com lógica de backoff exponencial.

5. **Escalabilidade**
   - O microserviço deve ser escalável horizontalmente, suportando um alto volume de mensagens e processamento simultâneo de vídeos.

---

## Estrutura do Projeto

O Projeto deverá seguir a implementação arquitetural no padrão Hexagonal.

Aqui está uma estrutura básica para implementar a **Arquitetura Hexagonal** no microserviço **ms-framevideo-app**:

---

### **Estrutura de Diretórios**

---

### **Estrutura de Diretórios**

```
ms-framevideo-app/
├── src/
│   ├── application/
│   │   ├── ports/
│   │   │   ├── FrameProcessorPort.cs
│   │   │   ├── MessagePublisherPort.cs
│   │   │   ├── StoragePort.cs
│   │   │   └── CachePort.cs
│   │   └── usecases/
│   │       ├── ProcessChunkUseCase.cs
│   │       └── UpdateFrameMetadataUseCase.cs
│   ├── domain/
│   │   ├── entities/
│   │   │   ├── Chunk.cs
│   │   │   └── Frame.cs
│   │   ├── exceptions/
│   │   │   └── FrameProcessingException.cs
│   │   └── services/
│   │       └── FrameGenerationService.cs
│   ├── infrastructure/
│   │   ├── adapters/
│   │   │   ├── RabbitMqPublisherAdapter.cs
│   │   │   ├── S3StorageAdapter.cs
│   │   │   ├── RedisCacheAdapter.cs
│   │   │   └── FrameProcessingAdapter.cs
│   │   ├── configuration/
│   │   │   ├── DependencyInjectionConfig.cs
│   │   │   └── AppSettings.json
│   │   └── framework/
│   │       └── KubernetesWorkerService.cs
│   ├── api/
│   │   └── FrameProcessingController.cs
│   └── Program.cs
├── tests/
│   ├── unit/
│   │   ├── FrameGenerationServiceTests.cs
│   │   └── ProcessChunkUseCaseTests.cs
│   ├── integration/
│   │   ├── RabbitMqIntegrationTests.cs
│   │   ├── S3StorageIntegrationTests.cs
│   │   └── RedisCacheIntegrationTests.cs
│   └── e2e/
│       └── FullFrameProcessingFlowTests.cs
├── kubernetes/
│   ├── deployment.yaml
│   ├── service.yaml
│   ├── hpa.yaml
└── README.md
```

---

### **Detalhamento da Estrutura**

#### **1. `domain` (Domínio)**
- Contém a lógica central e regras de negócio do microserviço.
- **Entidades (`entities/`)**:
  - `Chunk.cs`: Representa um pedaço de vídeo processado.
  - `Frame.cs`: Representa um frame gerado a partir de um chunk.
- **Serviços (`services/`)**:
  - `FrameGenerationService.cs`: Contém a lógica para gerar frames de chunks.
- **Exceções (`exceptions/`)**:
  - `FrameProcessingException.cs`: Captura erros específicos relacionados ao processamento de frames.

---

#### **2. `application` (Aplicação)**
- **Portas (`ports/`)**:
  - Interfaces que definem os contratos para as dependências externas.
  - Exemplos:
    - `FrameProcessorPort`: Define operações para processar chunks e gerar frames.
    - `MessagePublisherPort`: Abstrai a publicação de mensagens no RabbitMQ.
    - `StoragePort`: Define operações para interação com o S3.
    - `CachePort`: Define operações para gravação de dados no Redis.
- **Casos de Uso (`usecases/`)**:
  - **`ProcessChunkUseCase`**: Coordena o download do chunk, geração de frames, compactação e upload.
  - **`UpdateFrameMetadataUseCase`**: Atualiza os metadados no Redis.

---

#### **3. `infrastructure` (Infraestrutura)**
- Implementa as dependências externas, conectando-se às portas definidas na camada de aplicação.
- **Adaptadores (`adapters/`)**:
  - Implementam as portas para interação com dependências externas.
  - Exemplos:
    - `RabbitMqPublisherAdapter`: Envia mensagens para RabbitMQ.
    - `S3StorageAdapter`: Lida com download/upload no S3.
    - `RedisCacheAdapter`: Gerencia operações de leitura/escrita no Redis.
    - `FrameProcessingAdapter`: Integra a lógica de processamento de chunks (ex.: uso de bibliotecas de vídeo como FFmpeg).
- **Configuração (`configuration/`)**:
  - Gerencia injeção de dependências e configurações gerais.
- **Framework (`framework/`)**:
  - Configura o ambiente para execução no Kubernetes.

---

#### **4. `api` (API)**
- Exposição do microserviço por meio de uma API (caso necessário).
- **Controladores (`FrameProcessingController`)**:
  - Opcionalmente, permite requisições HTTP para acionar o processamento de chunks (debug, monitoramento, etc.).

---

#### **5. `tests`**
- **Unitários (`unit/`)**:
  - Testam os serviços e casos de uso de forma isolada.
- **Integração (`integration/`)**:
  - Validam a interação com dependências externas (S3, RabbitMQ, Redis).
- **Fim a Fim (`e2e/`)**:
  - Simulam o fluxo completo de processamento de chunks e geração de frames.

---

### **Fluxo de Trabalho**

1. **Recepção de Mensagem**:
   - O adaptador `RabbitMqPublisherAdapter` consome a mensagem e aciona o caso de uso `ProcessChunkUseCase`.

2. **Processamento do Chunk**:
   - O caso de uso:
     - Usa `StoragePort` para baixar o chunk do S3.
     - Usa `FrameGenerationService` para gerar os frames.
     - Compacta os frames em um arquivo `.zip`.

3. **Upload e Atualização**:
   - Após compactar:
     - Usa `StoragePort` para fazer upload do `.zip` ao S3.
     - Usa `CachePort` para atualizar informações no Redis.

4. **Publicação de Mensagem**:
   - Usa `MessagePublisherPort` para notificar que o `.zip` foi gerado.

---
