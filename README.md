# PPO training for competitive resource collection agents in Unity ML-Agents

Dependencies:
- Unity 2022.3 LTS+
- Python 3.9-3.10
- mlagents==4.0.0
- torch==2.0.1
- protobuf==3.20.1
- grpcio==1.75.1

Training Configuration (config.yaml):
- PPO algorithm with lr=0.0003, batch_size=2048
- Network: 3 layers × 256 hidden units
- Time horizon: 256, max steps: 3000000
- Optimized for competitive 2-agent environment

Reward System:
- Tree collection: +1.0
- Chest collection: +3.0
- Wall collision: -0.1
- Opponent penalty: 0.3 (when opponent scores, you get -0.3× their reward)
- Win reward: +5.0
- Loss penalty: -5.0
- Tie penalties: -10.0 (idle 0:0), -2.5 (active tie)

Run Training:
1. Build Unity project to TraningYaml/builds folder
2. mlagents-learn config.yaml --run-id=experiment_name --env=builds/YourBuild.exe
3. tensorboard --logdir results/ (monitor progress)

Agent Implementation: 73-dimensional observation space, 2 continuous actions, competitive reward shaping with opponent penalty

The arena:
<img width="955" height="535" alt="image" src="https://github.com/user-attachments/assets/1ef8dfd7-ee3e-49ba-864b-9b473d9ca7fb" />

The model after 1.6mln steps:

https://github.com/user-attachments/assets/dc0e15e1-acda-41ae-a15c-431ecaf50a07




