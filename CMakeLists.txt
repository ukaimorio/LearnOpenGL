cmake_minimum_required(VERSION 3.10)

project(hello_world)

file (GLOB SRC_FILES
    "${PROJECT_SOURCE_DIR}/src/02.Hello_Triangle/*.cpp"
    "${PROJECT_SOURCE_DIR}/src/02.Hello_Triangle/*.h"
)

add_executable(${CMAKE_PROJECT_NAME} ${SRC_FILES})

find_package(glad CONFIG REQUIRED)
find_package(glfw3 CONFIG REQUIRED)

target_link_libraries(${CMAKE_PROJECT_NAME} PRIVATE glad::glad)
target_link_libraries(${CMAKE_PROJECT_NAME} PRIVATE glfw)

target_compile_features(${CMAKE_PROJECT_NAME} PRIVATE cxx_std_17)
