from matplotlib import pyplot as plt

def draw_fusion_result(V,A):
    fig = plt.figure(figsize=(4, 4))
    ax = fig.add_subplot(111)
    ax.spines['top'].set_color('none')
    ax.spines['right'].set_color('none')
    ax.xaxis.set_ticks_position('bottom')
    ax.spines['bottom'].set_position(('data', 0))
    ax.yaxis.set_ticks_position('left')
    ax.spines['left'].set_position(('data', 0))
    for i in range(8):
        plt.plot(V[i]-5,A[i]-5)
    #plt.style.use('ggplot')
    ax.set_xticks([-4, 4])
    ax.set_yticks([-4, 4])    
    plt.show()