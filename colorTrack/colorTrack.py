import cv2
import numpy as np

#Below is the lower and upper bound value(HSV) for Cyan color to track. Google your desired color! 
lb = np.array([90, 50, 50])
ub = np.array([130, 255, 255])

#Run this only when called as a file,not from a module.
#Get frame->compute mask of desired color->get largest area contour from that->compute centroid and draw that. Simple!
def main():
    cap = cv2.VideoCapture(0)

    while(1):
        _, frame = cap.read()
        hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
        mask = cv2.inRange(hsv, lb, ub)
        cont, _ = cv2.findContours(mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        maxAreaCnt = sorted(cont, key = cv2.contourArea, reverse = True) 
        M = cv2.moments(maxAreaCnt[0])
        cx = int(M['m10']/M['m00'])
        cy = int(M['m01']/M['m00'])
        cv2.circle(frame, (cx,cy), 10, (0,255,255), 3)
        #cv2.drawContours(framecopy, maxAreaCnt, 0, (0,255,255), 3)
        #res = cv2.bitwise_and(frame, frame, mask = mask)
        #cv2.imshow('contours', framecopy)
        cv2.imshow('frame', frame)
        if(cv2.waitKey(10) == ord('q')):
            break

    cap.release()
    cv2.destroyAllWindows()

if __name__ == "__main__":
    main()