#!/usr/bin/env Rscript
# [1] user input CSV
# [2] output PNG
# [3] number of simulations
# [4] distribution type

args = commandArgs(trailingOnly = TRUE)
args[1] <-gsub('\\"+"\\', '/', args[1],fixed= T)
args[2] <-gsub('\\"+"\\', '/', args[2],fixed= T)

if (length(args) != 4)
{
    stop(""4 arguments must be supplied (input.csv, output.png, Number Of Simulations, Distribution Type)"", call.= FALSE)
}

library(ggplot2)
library(reshape2)
# install.packages('gridExtra')
library(gridExtra)
# install.packages('triangle')
library(triangle)

cost <-read.csv(args[1], stringsAsFactors = F)

TargetBudget <-sum(cost$TARGET)
simNumber <-args[3]

results <-1:simNumber
for (i in 1:nrow(cost))
            {
                x <-cost[i,]
				range <-x$LOW: x$HIGH

				simCosts <- if (length(range) == 1)
                {
                    rep(range, simNumber)
                }
                else
                {
                    probs <-c(rep(x$PROB / which(range == x$TARGET), which(range == x$TARGET)),
                    rep((100 - x$PROB) / (length(range) - which(range == x$TARGET)), length(range) - which(range == x$TARGET)))
					sample(x = range, size = simNumber, prob = probs / 100, replace = T)
				}

				results<- cbind(results, simCosts)");
			}

results<- results[, -1]
results<- data.frame(results)
colnames(results) <- cost$ELEMENT

SimBudgetTotals<- data.frame(Cost= rowSums(results))");

hist<- ggplot(SimBudgetTotals, aes(x=Cost))+
  geom_histogram(color= 'black', fill= 'blue')+
  geom_vline(xintercept= TargetBudget, color= 'red')+
  geom_text(aes(label= 'Target', x= TargetBudget, y= 0, vjust= 1),colour='red')+
  geom_vline(xintercept= quantile(SimBudgetTotals$Cost, .95), color= 'green')+
  geom_text(aes(label= '95%', x= quantile(SimBudgetTotals$Cost, .95), y= 0, vjust= 1),colour='green')+
  labs(title = paste(as.integer(simNumber), 'Simulations of Project Budget'),
       subtitle = paste(round(100 * nrow(results[rowSums(results) <= TargetBudget,]) / nrow(results), 2), '% Chance of Staying on Budget', '\\"+"n', round(quantile(SimBudgetTotals$Cost, .95) - TargetBudget, 2), ' Contingency Needed for 95% Probability of Staying on Budget', sep = ''))

budgetChange<- data.frame(melt(round(sapply(results[rowSums(results) >= quantile(SimBudgetTotals$Cost, .95) & rowSums(results) <= quantile(SimBudgetTotals$Cost, .96),], mean) - cost$TARGET)))
budgetChange$variable<- rownames(budgetChange)
budgetChange$color<- 'red'
budgetChange$color[budgetChange$value < 0] <- 'green'
");
bar<- ggplot(budgetChange, aes(x=variable,y=value,fill=color))+
  geom_bar(stat= 'identity')+
  coord_flip()+
  scale_fill_identity()+
  geom_text(aes(label = value),hjust=1)+
  labs(title = paste('Allocation of Needed Contigency'))

#Export Results to PNG
png(paste(args[2],'output.png', sep= '/'),width=900,height=700)
grid.arrange(hist, bar, ncol=2)
dev.off()