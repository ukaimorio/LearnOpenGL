#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include <iostream>

void framebuffer_size_callback(GLFWwindow *window, int width, int height);
void processInput(GLFWwindow *window);

const unsigned int SCR_WIDTH = 800;
const unsigned int SCR_HEIGHT = 600;

int main()
{
    glfwInit();
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    // 指定使用的是OpenGL 3.3版本
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
    // 指定使用的是核心模式(Core-profile)
    GLFWwindow *window = glfwCreateWindow(SCR_WIDTH, SCR_HEIGHT, "LearnOpenGL", NULL, NULL);
    // 创建窗口对象
    if (window == NULL)
    {
        std::cout << "Failed to create GLFW window" << std::endl;
        glfwTerminate();
        return -1;
    }
    glfwMakeContextCurrent(window);

    glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);
    // 注册窗口大小改变的回调函数
    if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
    {
        std::cout << "Failed to initialize GLAD" << std::endl;
        return -1;
    }
    // 初始化GLAD,传入的是GLAD用来加载系统相关的OpenGL函数指针地址的函数

    // 注册回调函数
    while (!glfwWindowShouldClose(window))
    {
        processInput(window);

        glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        // 设置清空屏幕所用的颜色
        glClear(GL_COLOR_BUFFER_BIT);
        // 清空颜色缓冲,当清空颜色缓冲后,整个颜色缓冲都会被填充为glClearColor所设置的颜色

        glfwSwapBuffers(window);
        glfwPollEvents();
        // 检查有没有触发什么事件(键盘输入、鼠标移动等),更新窗口状态,并调用对应的回调函数(可以通过回调方法手动设置)
    }
    glfwTerminate();
    return 0;
}
// 窗口大小改变时调用,width,height为新的宽度和高度
void framebuffer_size_callback(GLFWwindow *window, int width, int height)
{
    glViewport(0, 0, width, height);
    // 设置gl视口的维度
}
void processInput(GLFWwindow *window)
{
    if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS)
        glfwSetWindowShouldClose(window, true);
}