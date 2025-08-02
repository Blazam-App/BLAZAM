sudo apt-get update && \
  sudo apt-get install -y aspnetcore-runtime-8.0 && \
  sudo apt-get install -y unzip && \
  sudo apt-get install -y libldap2 && \
  sudo ln -s /usr/lib/x86_64-linux-gnu/libldap.so.2.0.200 /usr/lib/x86_64-linux-gnu/libldap-2.5.so.0
  cd ~ && \
  wget https://github.com/Blazam-App/BLAZAM/releases/download/Release-v1.4.0.2025.05.15.2236/blazam-stable-v1.4.0.2025.05.15.2236.zip && \
  mkdir app/ && \
  unzip blazam-stable-v1.4.0.2025.05.15.2236.zip -d app/ && \

