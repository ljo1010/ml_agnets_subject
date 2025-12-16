# ⚙️ Reinforcement Learning with ML-Agents in Unity

Unity ML-Agents를 활용하여 **생존 기반 환경에서 에이전트가 음식(Food)과 위험요소(Hazard)를 인식하고 학습하는 강화학습 프로젝트**입니다.  
에이전트는 시야(Ray Perception)와 커스텀 센서를 통해 환경을 관측하고, **허기(Hunger)와 체력(Health)을 유지하며 생존하는 정책**을 학습합니다.

---

## 📌 Project Overview

- **Engine**: Unity
- **RL Framework**: Unity ML-Agents (PPO)
- **Training Backend**: Python (PyTorch via ML-Agents)
- **Inference Model**: ONNX
- **Learning Type**: Reinforcement Learning (Survival Task)

### Goal
에이전트가 다음을 학습하는 것을 목표로 합니다.
- Food와 Hazard를 **구분하여 인식**
- Food를 찾아 이동하여 생존 시간 연장
- Hazard 및 Wall 회피
- 무의미한 정지/회전을 피하고 합리적인 탐험 수행

---

## 🧠 Learning Algorithm

- **Algorithm**: PPO (Proximal Policy Optimization)
- **Reason for PPO**
  - 안정적인 정책 업데이트
  - 하이퍼파라미터에 비교적 둔감
  - 다양한 게임 환경에서 검증된 성능
  - Unity ML-Agents의 기본 권장 알고리즘

---

## 🌍 Environment Design

### Objects
- **Agent**: 생존을 목표로 하는 AI 에이전트
- **Food**: 충돌 시 제거되고 랜덤 위치에 재생성
- **Hazard**: 충돌 시 패널티 부여
- **Wall**: 맵 경계, 충돌 시 패널티

### Sensors
- **Ray Perception Sensor**
  - Detectable Tags: `Food`, `Hazard`, `Wall`
- **Custom Vector Observations**
