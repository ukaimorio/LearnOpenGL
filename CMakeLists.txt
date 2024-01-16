cmake_minimum_required(VERSION 3.10)

project(opengl)

include_directories(include)


file (GLOB SRC_FILES
    "${PROJECT_SOURCE_DIR}/include/*.h"
    "${PROJECT_SOURCE_DIR}/src/6.PBR/3.IBL/4.Specular_IBL_model/*.cpp"
)

add_executable(${CMAKE_PROJECT_NAME} ${SRC_FILES})

find_package(glad CONFIG REQUIRED)
find_package(glfw3 CONFIG REQUIRED)
find_package(glm CONFIG REQUIRED)
find_package(assimp CONFIG REQUIRED)

target_link_libraries(${CMAKE_PROJECT_NAME} PRIVATE glad::glad)
target_link_libraries(${CMAKE_PROJECT_NAME} PRIVATE glfw)
target_link_libraries(${CMAKE_PROJECT_NAME} PRIVATE glm::glm)
target_link_libraries(${CMAKE_PROJECT_NAME} PRIVATE assimp::assimp)

target_compile_features(${CMAKE_PROJECT_NAME} PRIVATE cxx_std_17)
