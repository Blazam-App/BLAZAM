sudo apt-get update && \
  sudo apt-get install -y aspnetcore-runtime-8.0 && \
  sudo apt-get install -y unzip && \
  cd ~ && \
  wget https://github.com/Blazam-App/BLAZAM/releases/download/Release-v1.4.0.2025.05.15.2236/blazam-stable-v1.4.0.2025.05.15.2236.zip && \
  mkdir app/ && \
  unzip blazam-stable-v1.4.0.2025.05.15.2236.zip -d app/ && \

